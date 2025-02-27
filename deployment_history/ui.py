import time
from functools import partial
from multiprocessing import Pool
from typing import Dict, Optional

from rich.console import Console, Group
from rich.live import Live
from rich.markup import escape
from rich.progress import Progress, SpinnerColumn, TextColumn
from rich.table import Table

from .constants import UWP_CD_SERVICE_USER_EMAIL, environments
from .core import get_deployments_for_env, get_gh_actions_workflow_runs
from .types import (
    DeploymentDetail,
    EnvironmentName,
    ProgressTask,
    RunNumber,
    TimeZone,
    WorkflowRunDetail,
)
from .utils import (
    get_formatted_datetime_string,
    is_deployment_associated_with_workflow_run,
)

console = Console()


def get_deployments(repo: str):
    deployments: Dict[EnvironmentName, Dict[RunNumber, DeploymentDetail]] = {}

    progress_tasks = {
        env: ProgressTask(
            progress=Progress(
                SpinnerColumn(),
                TextColumn(
                    f"Fetching [cyan bold]{env}[/] environmemt...",
                ),
                TextColumn(
                    "{task.description}",
                ),
            ),
            task_id=None,
        )
        for env in environments
    }

    progress_group = Group(
        "[cyan bold]Resolving history for environments...[/]",
        *[progress_tasks[env].progress for env in environments],
    )

    live = Live(
        progress_group,
        transient=True,
        refresh_per_second=10,
    )

    with live:
        # Initial rendering of progress
        for env in environments:
            progress_task = progress_tasks[env]
            task_id = progress_task.progress.add_task(
                description="[yellow bold]In Progress[/]",
            )
            progress_task.task_id = task_id

        history_resolution_start_time = time.time()

        with Pool() as pool:
            results = pool.imap_unordered(
                partial(get_deployments_for_env, repo=repo),
                environments,
            )

            for env, env_deployments in results:
                deployments[env] = env_deployments

                # Update progress status
                progress_task = progress_tasks[env]
                task_id = progress_task.task_id

                if task_id is not None:
                    progress_task.progress.update(
                        task_id=task_id,
                        description="[green bold]Done[/]",
                        completed=True,
                    )

        live.console.log(
            " ".join(
                [
                    "[blue bold][INFO][/]",
                    "Resolved history for environments in",
                    f"{time.time() - history_resolution_start_time:.2f}",
                    "secs",
                ]
            ),
        )

    return deployments


def show_deployment_history(repo: str, detailed: bool, timezone: TimeZone):
    workflows_fetch_progress = Progress(
        SpinnerColumn(),
        TextColumn("Fetching workflow runs from GitHub..."),
    )
    live = Live(
        workflows_fetch_progress,
        transient=True,
        refresh_per_second=10,
    )
    workflows_fetch_progress_task_id = workflows_fetch_progress.add_task("")
    workflows_fetch_start_time = time.time()

    with live:
        action_workflows = get_gh_actions_workflow_runs(repo)

        workflows_fetch_progress.update(
            task_id=workflows_fetch_progress_task_id,
            completed=True,
        )
        live.console.log(
            " ".join(
                [
                    "[blue bold][INFO][/]",
                    "Fetched workflow runs from GitHub in",
                    f"{time.time() - workflows_fetch_start_time:.2f}",
                    "secs",
                ]
            ),
        )

    deployments = get_deployments(repo)

    table = Table(
        padding=1,
        row_styles=["on grey11", ""],
    )

    table.add_column("#", justify="right")
    table.add_column("Workflow Run", justify="left")
    table.add_column("Int", justify="center")
    table.add_column("Dev", justify="center")
    table.add_column("QA", justify="center")
    table.add_column("UAT", justify="center")
    table.add_column("NPD", justify="center")
    table.add_column("Prod", justify="center")

    for i, workflow in enumerate(action_workflows):
        table.add_row(
            str(i + 1),
            get_commit_detailed_text(workflow, deployments, detailed, timezone),
            get_cell_value_for_env_status(workflow, deployments.get("int")),
            get_cell_value_for_env_status(workflow, deployments.get("dev")),
            get_cell_value_for_env_status(workflow, deployments.get("qa")),
            get_cell_value_for_env_status(workflow, deployments.get("uat")),
            get_cell_value_for_env_status(workflow, deployments.get("npd")),
            get_cell_value_for_env_status(workflow, deployments.get("prod")),
        )

    console.print(table)


def get_commit_detailed_text(
    workflow: WorkflowRunDetail,
    deployments: Dict[str, Dict[str, DeploymentDetail]],
    detailed: bool,
    timezone: TimeZone,
):
    commit_message_summary = workflow.commit_message.splitlines()[0]
    commit_hash = workflow.commit_hash[:7]
    run_number = workflow.run_number
    workflow_started_datetime = get_formatted_datetime_string(
        workflow.started_at,
        timezone,
    )

    if detailed:
        deployment_details = []

        for env in environments:
            env_deployments = deployments[env]
            deployment_detail = env_deployments.get(run_number, None)

            is_deployed_to_env = (
                deployment_detail is not None
                and is_deployment_associated_with_workflow_run(
                    workflow,
                    deployment_detail,
                )
            )

            if not is_deployed_to_env:
                continue

            # Explicit type assertion for Pyright
            assert deployment_detail is not None

            commit_relative_date = deployment_detail.commit_relative_date
            commit_author_email = deployment_detail.commit_author_email
            commit_author_name = deployment_detail.commit_author_name
            commit_date = get_formatted_datetime_string(
                deployment_detail.commit_date,
                timezone,
            )

            if commit_author_email == UWP_CD_SERVICE_USER_EMAIL:
                deployment_details.append(
                    " ".join(
                        [
                            ":white_check_mark:",
                            f"Deployed to [bright_magenta bold]{env}[/] on",
                            f"[cyan bold]{commit_date}[/]",
                            f"[cyan italic]({commit_relative_date})[/]",
                        ]
                    )
                )
            else:
                deployment_status_message = " ".join(
                    [
                        ":blue_square:",
                        f"Possibly deployed to [bright_magenta bold]{env}[/]",
                        "on or after",
                        f"[cyan bold]{commit_date}[/]",
                        f"[cyan italic]({commit_relative_date})[/]",
                    ]
                )

                deployment_details.append(
                    "\n".join(
                        [
                            deployment_status_message,
                            " ".join(
                                [
                                    "\t[white bold]Manually tagged by:[/]",
                                    commit_author_name,
                                    f"({commit_author_email})",
                                ]
                            ),
                        ]
                    )
                )

        workflow_committer_email = workflow.committer.get("email", "unknown_email")
        workflow_author_email = workflow.author.get("email", "unknown_email")

        return "\n".join(
            [
                " ".join(
                    [
                        "".join(
                            [
                                f"[link={workflow.url}]",
                                escape(workflow.commit_message),
                                "[/link]",
                            ]
                        ),
                        f"[blue bold](#{workflow.run_number})[/]",
                    ]
                ),
                "\n".join(
                    [
                        "",
                        " ".join(
                            [
                                "[white bold]Commit Hash:[/]",
                                f"[yellow]{workflow.commit_hash}[/]",
                            ]
                        ),
                        " ".join(
                            [
                                "[white bold]Author:[/]",
                                workflow.author.get("name", "Unknown"),
                                f"({workflow_author_email})",
                            ]
                        ),
                        " ".join(
                            [
                                "[white bold]Committer:[/]",
                                workflow.committer.get("name", "Unknown"),
                                f"({workflow_committer_email})",
                            ]
                        ),
                        " ".join(
                            [
                                "[white bold]Branch:[/]",
                                f"[bright_magenta]{workflow.branch}[/]",
                            ]
                        ),
                        " ".join(
                            [
                                "[white bold]Workflow URL:[/]",
                                f"[blue bold underline]{workflow.url}[/]",
                            ]
                        ),
                        " ".join(
                            [
                                "Triggered via",
                                f"[cyan]{workflow.event}[/]",
                                "at",
                                f"[cyan bold]{workflow_started_datetime}[/]",
                            ]
                        ),
                        "",
                        (
                            "\n".join(deployment_details)
                            if len(deployment_details) != 0
                            else "[bright_black italic]No deployment details[/]"
                        ),
                    ]
                ),
            ]
        )

    return "\n".join(
        [
            " ".join(
                [
                    "".join(
                        [
                            f"[link={workflow.url}]",
                            escape(commit_message_summary),
                            "[/link]",
                        ]
                    ),
                    f"[blue bold](#{workflow.run_number})[/]",
                ]
            ),
            " ".join(
                [
                    f"[yellow bold]\\[{commit_hash}][/]",
                    "-",
                    f"[white bold]{workflow.author.get('name')}[/]",
                    "-",
                    f"[bright_magenta bold]{workflow.branch}[/]",
                    "-",
                    f"Triggered via [cyan]{workflow.event}[/]",
                    "at",
                    f"[cyan bold]{workflow_started_datetime}[/]",
                ]
            ),
        ]
    )


def get_cell_value_for_env_status(
    workflow_run: WorkflowRunDetail,
    env_deployments: Optional[Dict[str, DeploymentDetail]] = None,
):
    run_number = workflow_run.run_number

    if env_deployments is None:
        env_deployments = {}

    deployment = env_deployments.get(run_number, None)

    is_deployed_to_env = (
        deployment is not None
        and is_deployment_associated_with_workflow_run(workflow_run, deployment)
    )

    if is_deployed_to_env:
        # Explicit type assertion to ensure Pyright does not throw type errors
        # for accessing attribute of None type even when `deployment` is not None
        assert deployment is not None

        return (
            "[bright_green]:white_check_mark:[/]"
            if deployment.commit_author_email == UWP_CD_SERVICE_USER_EMAIL
            else "[blue]:blue_square:[/]"
        )

    return "[bright_black]-[/]"
