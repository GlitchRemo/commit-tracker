from dataclasses import dataclass
from enum import Enum

from typing_extensions import Literal, Optional, TypeVar, Union


T = TypeVar("T")


class PortForwardStatus(Enum):
    NONE = "NONE"
    STARTING = "STARTING"
    UP = "UP"
    DOWN = "DOWN"
    HANDLING_REQUEST = "HANDLING_REQUEST"


@dataclass
class PortForwardStatusMessage:
    type: Literal["portforward-status"]
    service: str
    status: PortForwardStatus
    message: Optional[str] = None


@dataclass
class KubeDeploymentStatusMessage:
    type: Literal["kube-deployment-status"]
    service: str
    numberOfPodsUp: int
    maxPods: int


Message = Union[
    PortForwardStatusMessage,
    KubeDeploymentStatusMessage,
]


@dataclass
class ServiceDetail:
    service: str
    port: int
    status: PortForwardStatus
    numberOfPodsUp: int
    maxPods: int
    numberOfRequestsHandled: int
