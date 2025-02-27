import subprocess

from typing_extensions import Iterator, List, Tuple

from .types import KubeDeploymentStatusMessage


class KubernetesDeploymentWatcher:
    def __init__(self, env: str, services: List[str]) -> None:
        self.env = env
        self.services = services

    def get_delpoyment_watch_command(self):
        return " ".join(
            [
                "kubectl get deployment",
                '--context "225063-dev-eus2-duwp-aks-01"',
                '--namespace "cxp-uwp2-dev"',
                "--watch",
            ]
        )

    @staticmethod
    def extract_data_from_command_output(
        command_output_line: str,
    ) -> Tuple[str, int, int]:
        service, readyPods, *_ = command_output_line.split()

        if "/" not in readyPods:
            return "", 0, 0

        numberOfPodsUp, maxPods = readyPods.split("/")

        return service, int(numberOfPodsUp), int(maxPods)

    def execute(self) -> Iterator[KubeDeploymentStatusMessage]:
        process = subprocess.Popen(
            self.get_delpoyment_watch_command(),
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            encoding="utf-8",
        )

        assert process.stdout is not None
        assert process.stderr is not None

        for line in iter(process.stdout.readline, ""):
            service, numberOfPodsUp, maxPods = (
                KubernetesDeploymentWatcher.extract_data_from_command_output(
                    line,
                )
            )

            if service in self.services:
                yield KubeDeploymentStatusMessage(
                    type="kube-deployment-status",
                    service=service,
                    numberOfPodsUp=numberOfPodsUp,
                    maxPods=maxPods,
                )

            if process.poll() is not None:
                break
