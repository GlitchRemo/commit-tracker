import json
import os
import subprocess
from typing import Any, Dict

import yaml

from .constants import UWP_CD_SERVICE_USER_EMAIL, repodir
from .types import (
    DeploymentDetail,
    RunNumber,
    WorkflowRunDetail,
)
from .utils import get_utc_datetime_from_iso_string


def get_gh_actions_workflow_runs(repo: str) -> list[WorkflowRunDetail]:
    rest_api_endpoint = "".join(
        [
            "https://api.github.com/",
            f"repos/nationalgrid-customer/{repo}/actions/runs",
        ]
    )

    workflow_runs_json_string = subprocess.getoutput(
        f'gh api "{rest_api_endpoint}" --method GET',
    )

    workflows_runs_json = json.loads(workflow_runs_json_string)
    workflow_runs = workflows_runs_json["workflow_runs"]
    workflow_run_details = []

    for workflow_run in workflow_runs:
        if workflow_run.get("path") not in [
            ".github/workflows/cicd.yml",
            ".github/workflows/ci.yml",
        ]:
            continue

        run_number = workflow_run.get("run_number")
        commit_hash = workflow_run.get("head_sha")
        commit_message = workflow_run.get("head_commit").get("message")
        author = workflow_run.get("head_commit").get("author")
        committer = workflow_run.get("head_commit").get("committer")
        event = workflow_run.get("event")
        branch = workflow_run.get("head_branch")
        started_at = workflow_run.get("run_started_at")
        url = workflow_run.get("html_url")

        workflow_run_details.append(
            WorkflowRunDetail(
                run_number=str(run_number),
                commit_hash=commit_hash,
                commit_message=commit_message,
                author=author,
                committer=committer,
                event=event,
                branch=branch,
                started_at=get_utc_datetime_from_iso_string(started_at),
                url=url,
            )
        )

    return workflow_run_details


def get_deployments_for_env(env: str, repo: str):
    env_deployments: Dict[RunNumber, DeploymentDetail] = {}
    currdir = os.getcwd()
    os.chdir(repodir)

    command = "".join(
        [
            'git log --format="%h::%ae::%an::%ci::%cr" -- ',
            f"k8s/{env}/{repo}/kustomization.yaml",
        ]
    )

    kustomization_commits_log = subprocess.getoutput(command)
    commit_lines = kustomization_commits_log.splitlines()
    kustomization_commits_meta_list = [
        commit_line.split("::") for commit_line in commit_lines
    ]

    for (
        commit_hash,
        commit_author_email,
        commit_author_name,
        commit_date,
        commit_relative_date,
    ) in kustomization_commits_meta_list:
        component_image_tags = get_kustomization_deployments_from_hash(
            commit_hash,
            env,
            repo,
        )

        if len(component_image_tags) == 0:
            # Skip mapping components' image tags if the list is empty. This
            # would happen if for the given commit hash, kustomization.yaml
            # file has components with tag not in correct format
            # (`date.run_number`)
            continue

        _, run_number = component_image_tags[0]

        if (
            run_number not in env_deployments
            or env_deployments[run_number].commit_author_email
            != UWP_CD_SERVICE_USER_EMAIL
        ):
            env_deployments[run_number] = DeploymentDetail(
                components={
                    component_name
                    for component_name, _ in component_image_tags
                    if component_name is not None
                },
                commit_date=get_utc_datetime_from_iso_string(commit_date),
                commit_relative_date=commit_relative_date,
                commit_author_name=commit_author_name,
                commit_author_email=commit_author_email,
            )

    os.chdir(currdir)
    return env, env_deployments


def get_kustomization_deployments_from_hash(hash: str, env: str, repo: str):
    command = f"git show {hash}:k8s/{env}/{repo}/kustomization.yaml"
    kustomization_content = subprocess.getoutput(command)

    kustomization_data: Dict[str, Any] = yaml.safe_load(kustomization_content)
    images: list[Dict[str, str]] = kustomization_data.get("images", [])

    component_image_tags = [
        (
            image.get("name"),
            image.get("newTag", "").split(".")[1],
        )
        for image in images
        # Skip component if image tag is not in correct format like `IMAGE_TAG`
        # instead of `date.run_number` or `date.run_number.commit_hash`
        if image.get("newTag", "").count(".") > 0
    ]

    return component_image_tags


def get_valid_repository_names():
    currdir = os.getcwd()
    os.chdir(repodir)

    k8s_dev_dir = os.path.join("k8s", "dev")
    k8s_dev_dir_contents = os.listdir(k8s_dev_dir)

    valid_repos = [
        repo_name
        for repo_name in k8s_dev_dir_contents
        if not os.path.isfile(os.path.join(k8s_dev_dir, repo_name))
    ]

    os.chdir(currdir)
    return valid_repos
