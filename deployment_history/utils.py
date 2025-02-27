import os
import subprocess
from datetime import datetime, timezone
from pathlib import Path

from rich.console import Console

from .constants import repodir, timezone_map
from .types import DeploymentDetail, TimeZone, WorkflowRunDetail

console = Console()


def ensure_repo_cloned():
    if os.path.exists(repodir):
        return True

    try:
        console.log("[blue bold][INFO][/] Cloning repo: cxp-deployment")

        repodir_parent = os.path.dirname(repodir)
        Path(repodir_parent).mkdir(parents=True, exist_ok=True)

        # Clone `cxp-deployment` to target directory
        git_clone_result = subprocess.run(
            [
                "git",
                "clone",
                "git@github.com:nationalgrid-customer/cxp-deployment.git",
                repodir,
            ]
        )

        if git_clone_result.returncode != 0:
            console.log(
                "[red bold][ERROR][/]",
                "git clone exited with return code",
                git_clone_result.returncode,
            )
            return False

    except Exception as error:
        console.log(f"[red bold][ERROR][/] {error}")
        return False

    return True


def update_repo():
    currdir = os.getcwd()

    try:
        console.log(
            "[blue bold][INFO][/]",
            "Pulling latest commits for cxp-deployment",
        )

        os.chdir(repodir)
        git_pull_result = subprocess.run(["git", "pull"])

        if git_pull_result.returncode != 0:
            console.log(
                "[red bold][ERROR][/]",
                "git pull exited with return code",
                git_pull_result.returncode,
            )
            return False

    except Exception as error:
        console.log(f"[red bold][ERROR][/] {error}")
        return False

    finally:
        os.chdir(currdir)

    return True


def get_utc_datetime_from_iso_string(iso_datetime_string: str):
    parsed_datetime = datetime.fromisoformat(iso_datetime_string)
    return parsed_datetime.astimezone(timezone.utc)


def get_formatted_datetime_string(
    datetime_obj: datetime,
    tz: TimeZone = TimeZone.utc,
):
    tz_zoneinfo = timezone_map[tz]
    converted_datetime = datetime_obj.astimezone(tz_zoneinfo)

    return converted_datetime.strftime("%a, %d %b %Y %H:%M:%S %z %Z")


def is_deployment_associated_with_workflow_run(
    workflow_run: WorkflowRunDetail,
    deployment: DeploymentDetail,
):
    return deployment.commit_date >= workflow_run.started_at
