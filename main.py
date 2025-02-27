from typer import Typer, Argument, Option
from enum import Enum
from typing_extensions import Annotated, Optional
from compare_images.compare_images_kubectl import main as compare_images_kubectl
from compare_images.compare_images_cxp_deployment import (
    main as compare_images_cxp_deployment,
)
from deployment_history.main import main as deployment_history_module
from deployment_history.types import TimeZone
from kube_portforward.main import main as kube_portforward_module

app = Typer(add_completion=False)
kube_portforward_app = Typer(
    name="kube-portforward",
    help="Manage port-forwarding for services in kubernetes",
)

app.add_typer(kube_portforward_app)


class Environment(str, Enum):
    int = "int"
    dev = "dev"
    qa = "qa"
    uat = "uat"
    npd = "npd"
    prod = "prod"


class CompareImagesScriptStrategy(str, Enum):
    kubectl = "kubectl"
    cxp_deployment = "cxp-deployment"


@app.command("compare-images", help="Compare image versions between environments")
def compare_images(
    reference_env: Annotated[
        Environment,
        Argument(help="Environment to be taken as reference for image versions"),
    ],
    target_env: Annotated[
        Environment,
        Argument(help="Environment to be compared with for image versions"),
    ],
    strategy: Annotated[
        CompareImagesScriptStrategy,
        Option(help="Strategy to be used to compare images across environments"),
    ] = CompareImagesScriptStrategy.cxp_deployment,
    export_csv_filepath: Annotated[
        Optional[str],
        Option(
            "--export-csv",
            help="Filename or path to CSV file to export the image versions report",
            exists=False,
        ),
    ] = None,
):
    if strategy == CompareImagesScriptStrategy.kubectl:
        compare_images_kubectl(
            reference_env,
            target_env,
            export_csv_filepath,
        )
    else:
        compare_images_cxp_deployment(
            reference_env,
            target_env,
            export_csv_filepath,
        )


@app.command("deployment-history", help="View deployment history for a given repo")
def deployment_history(
    repo: Annotated[
        str,
        Argument(
            help="Repository name for which deployment history needs to be viewed",
        ),
    ],
    detailed: Annotated[
        bool,
        Option(
            "--detailed",
            help="Shows detailed info when displaying workflow runs",
        ),
    ] = False,
    timezone: Annotated[
        TimeZone,
        Option(
            "--timezone",
            "--tz",
            help="Preferred timezone for displaying dates",
        ),
    ] = TimeZone.utc,
):
    deployment_history_module(repo, detailed, timezone)


@kube_portforward_app.command(
    "start",
    help="Start portforwarding of services (as provided in configuration) for given environment",
)
def kube_portforward_start(
    env: Annotated[
        Environment,
        Argument(
            help="Environment for which the port-forwarding needs to be started",
        ),
    ],
):
    kube_portforward_module(env)


if __name__ == "__main__":
    app()
