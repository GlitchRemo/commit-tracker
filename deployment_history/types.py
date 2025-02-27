from dataclasses import dataclass
from datetime import datetime
from typing import Dict, Optional, Set
from enum import Enum

from rich.progress import Progress, TaskID


@dataclass
class WorkflowRunDetail:
    run_number: str
    commit_hash: str
    commit_message: str
    author: Dict[str, str]
    committer: Dict[str, str]
    event: str
    branch: str
    started_at: datetime
    url: str


@dataclass
class DeploymentDetail:
    components: Set[str]
    commit_date: datetime
    commit_relative_date: str
    commit_author_name: str
    commit_author_email: str


@dataclass
class ProgressTask:
    progress: Progress
    task_id: Optional[TaskID]


class TimeZone(str, Enum):
    local = "local"
    utc = "utc"
    eastern = "eastern"


EnvironmentName = str
RunNumber = str
