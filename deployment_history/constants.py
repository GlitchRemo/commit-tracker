import os
from pathlib import Path
from typing import Dict, Optional

from zoneinfo import ZoneInfo

from .types import TimeZone

repodir = os.path.join(Path.home(), ".config", "ngrid", "cxp-deployment")
environments = ["int", "dev", "qa", "uat", "npd", "prod"]
UWP_CD_SERVICE_USER_EMAIL = "UWP2.0@nationalgridplc.onmicrosoft.com"

timezone_map: Dict[TimeZone, Optional[ZoneInfo]] = {
    TimeZone.utc: ZoneInfo("UTC"),
    TimeZone.eastern: ZoneInfo("US/Eastern"),
    TimeZone.local: None,
}
