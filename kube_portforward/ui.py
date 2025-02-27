from concurrent.futures import ThreadPoolExecutor
from queue import Empty, Queue

from rich.console import Console, ConsoleOptions, RenderableType
from rich.live import Live
from rich.table import Table
from typing_extensions import Dict, Iterable, Optional

from .types import (
    KubeDeploymentStatusMessage,
    Message,
    PortForwardStatus,
    PortForwardStatusMessage,
    ServiceDetail,
    T,
)
from .workers import deployment_watcher_worker, portforward_worker


class PortForwardTable:
    table: Optional[Table]
    services: Dict[str, ServiceDetail]

    def __init__(self):
        self.table = None
        self.services = {}

    @staticmethod
    def _get_service_cell_content(serviceDetail: ServiceDetail):
        numberOfPodsUpText = (
            f"[bright_red bold]{serviceDetail.numberOfPodsUp}[/]"
            if serviceDetail.numberOfPodsUp == 0
            else f"[blue bold]{serviceDetail.numberOfPodsUp}[/]"
        )

        return "\n".join(
            [
                f"[bold]{serviceDetail.service}[/]",
                "",
                f":package: {numberOfPodsUpText}/{serviceDetail.maxPods} pods are running",
                f":white_check_mark: Handled [cyan bold]{serviceDetail.numberOfRequestsHandled}[/] requests in total",
            ]
        )

    @staticmethod
    def _get_status_cell_content(status: PortForwardStatus):
        match status:
            case PortForwardStatus.STARTING:
                return "[yellow]:yellow_circle: Starting[/]"

            case PortForwardStatus.UP:
                return "[green]:green_circle: Running[/]"

            case PortForwardStatus.DOWN:
                return "[bright_red]:cross_mark: Down[/]"

            case _:
                return "[bright_black]Unknown[/]"

    def build_table(self):
        self.table = Table(
            padding=1,
            row_styles=["on grey11", ""],
            expand=True,
        )

        self.table.add_column("Service", justify="left")
        self.table.add_column("Port", justify="left")
        self.table.add_column("Status", justify="left", min_width=20)

        for _, detail in self.services.items():
            self.table.add_row(
                PortForwardTable._get_service_cell_content(detail),
                str(detail.port),
                PortForwardTable._get_status_cell_content(detail.status),
            )

    def __rich_console__(
        self,
        console: Console,
        options: ConsoleOptions,
    ) -> Iterable[RenderableType]:
        if self.table:
            yield self.table


def get_queue_data(q: Queue[T]):
    while True:
        try:
            return q.get(timeout=1)  # Allow check for Ctrl-C every second
        except Empty:
            pass


def render(env: str, services_ports_map: Dict[str, int]):
    portforward_table = PortForwardTable()

    message_queue_in: Queue[Message] = Queue()
    pool = ThreadPoolExecutor(max_workers=len(services_ports_map) + 1)

    pool.submit(
        deployment_watcher_worker,
        env,
        list(services_ports_map.keys()),
        message_queue_in,
    )

    for service, port in services_ports_map.items():
        pool.submit(portforward_worker, env, service, port, message_queue_in)

        portforward_table.services[service] = ServiceDetail(
            service=service,
            port=port,
            status=PortForwardStatus.NONE,
            numberOfPodsUp=0,
            maxPods=0,
            numberOfRequestsHandled=0,
        )

    pool.shutdown(wait=False)

    live = Live(
        renderable=portforward_table,
        auto_refresh=True,
        refresh_per_second=1,
    )

    with live:
        for message in iter(lambda: get_queue_data(message_queue_in), None):
            if message.type == "portforward-status":
                assert isinstance(message, PortForwardStatusMessage)

                serviceDetail = portforward_table.services[message.service]

                match message.status:
                    case (
                        PortForwardStatus.STARTING
                        | PortForwardStatus.UP
                        | PortForwardStatus.DOWN
                    ):
                        serviceDetail.status = message.status

                    case PortForwardStatus.HANDLING_REQUEST:
                        serviceDetail.numberOfRequestsHandled += 1

                portforward_table.build_table()
                live.refresh()

            elif message.type == "kube-deployment-status":
                assert isinstance(message, KubeDeploymentStatusMessage)

                serviceDetail = portforward_table.services[message.service]

                serviceDetail.numberOfPodsUp = message.numberOfPodsUp
                serviceDetail.maxPods = message.maxPods

                portforward_table.build_table()
                live.refresh()
