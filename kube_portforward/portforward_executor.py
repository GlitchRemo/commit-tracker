import subprocess

from typing_extensions import Iterator

from .types import PortForwardStatus, PortForwardStatusMessage


class PortforwardExecutor:
    def __init__(self, env: str, deployment_name: str, port: int) -> None:
        self.env = env
        self.deployment_name = deployment_name
        self.port = port

    def get_portforward_command(self):
        return " ".join(
            [
                "kubectl port-forward",
                '--context "225063-dev-eus2-duwp-aks-01"',
                '--namespace "cxp-uwp2-dev"',
                f"deployment/{self.deployment_name} {self.port}:8080",
            ]
        )

    @staticmethod
    def _has_started_portforward(command_output_line: str):
        return "Forwarding from" in command_output_line

    @staticmethod
    def _is_handling_portforward_request(command_output_line: str):
        return "Handling connection" in command_output_line

    def execute(self) -> Iterator[PortForwardStatusMessage]:
        process = subprocess.Popen(
            self.get_portforward_command(),
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            encoding="utf-8",
        )

        yield PortForwardStatusMessage(
            type="portforward-status",
            service=self.deployment_name,
            status=PortForwardStatus.STARTING,
        )

        assert process.stdout is not None
        assert process.stderr is not None

        for line in iter(process.stdout.readline, ""):
            if PortforwardExecutor._has_started_portforward(line):
                yield PortForwardStatusMessage(
                    type="portforward-status",
                    service=self.deployment_name,
                    status=PortForwardStatus.UP,
                )
            elif PortforwardExecutor._is_handling_portforward_request(line):
                yield PortForwardStatusMessage(
                    type="portforward-status",
                    service=self.deployment_name,
                    status=PortForwardStatus.HANDLING_REQUEST,
                )

            if process.poll() is not None:
                break

        if process.stderr.readable():
            yield PortForwardStatusMessage(
                type="portforward-status",
                service=self.deployment_name,
                status=PortForwardStatus.DOWN,
                message=process.stderr.read(),
            )
