from rich.console import Console

from .core import get_valid_repository_names
from .types import TimeZone
from .ui import show_deployment_history
from .utils import (
    ensure_repo_cloned,
    update_repo,
)

console = Console()


def main(repo: str, detailed: bool, timezone: TimeZone):
    is_repo_cloned = ensure_repo_cloned()

    if not is_repo_cloned:
        console.log(
            "[red bold][ERROR][/]",
            "Exiting due to failure while cloning cxp-deployment",
        )
        exit(1)

    did_update_repo = update_repo()

    if not did_update_repo:
        console.log(
            "[red bold][ERROR][/]",
            "Exiting due to failure while pulling latest changes for cxp-deployment",
        )
        exit(1)

    valid_repos = get_valid_repository_names()

    if repo not in valid_repos:
        console.log(
            "[red bold][ERROR][/]",
            "Repository name provided is not valid",
        )

        console.print(
            "\n[white bold]Consider providing one of the following repository names:[/]\n",
        )
        console.print(
            "\n".join(
                sorted(
                    map(
                        lambda repo_name: f"- {repo_name}",
                        valid_repos,
                    )
                ),
            ),
        )
        exit(1)

    print()
    show_deployment_history(repo, detailed, timezone)
