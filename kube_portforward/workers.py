from queue import Queue

from typing_extensions import List

from .kube_deployment_watcher import KubernetesDeploymentWatcher
from .portforward_executor import PortforwardExecutor
from .types import Message


def portforward_worker(
    env: str, deployment_name: str, port: int, message_queue_out: Queue[Message]
):
    portforward_executor = PortforwardExecutor(
        env,
        deployment_name,
        port,
    )

    for message in portforward_executor.execute():
        message_queue_out.put(message)


def deployment_watcher_worker(
    env: str,
    services: List[str],
    message_queue_out: Queue[Message],
):
    deployment_watcher = KubernetesDeploymentWatcher(
        env,
        services,
    )

    for message in deployment_watcher.execute():
        message_queue_out.put(message)
