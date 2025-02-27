from sys import argv
from pathlib import Path
import subprocess
import os
import json
import yaml
from typing import Any, Dict, Optional
from colorama import just_fix_windows_console, Fore
from .util import summarize

# Fix colored output in Windows Console (Legacy) by
# preventing ANSI code gibberish from being printed on
# the console and instead get colored output. Might still
# have minor caveats with respect to font styles
just_fix_windows_console()


__dir__ = os.path.dirname(__file__)
repodir = os.path.join(Path.home(), ".config", "ngrid", "cxp-deployment")
config = {}


with open(f"{__dir__}/config.json") as f:
    config = json.loads(f.read())


def ensure_repo_cloned():
    if os.path.exists(repodir):
        return True

    try:
        info_prefix = f"{Fore.LIGHTBLUE_EX}[INFO]{Fore.RESET}"
        print(f"{info_prefix} Cloning repo: cxp-deployment")

        repodir_parent = os.path.dirname(repodir)
        Path(repodir_parent).mkdir(parents=True, exist_ok=True)

        # Clone `cxp-deployment` to target directory
        git_clone_result = subprocess.run(
            [
                "git",
                "clone",
                "git@github.com:nationalgrid-customer/cxp-deployment.git",
                repodir,
            ]
        )

        if git_clone_result.returncode != 0:
            print(
                f"{Fore.LIGHTRED_EX}[ERROR]{Fore.RESET} git clone exited with return code {git_clone_result.returncode}"
            )
            return False

    except Exception as error:
        print(f"{Fore.LIGHTRED_EX}[ERROR]{Fore.RESET} {error}")
        return False

    return True


def update_repo():
    currdir = os.getcwd()

    try:
        info_prefix = f"{Fore.LIGHTBLUE_EX}[INFO]{Fore.RESET}"
        print(f"{info_prefix} Pulling latest commits for cxp-deployment")

        os.chdir(repodir)
        git_pull_result = subprocess.run(["git", "pull"])

        if git_pull_result.returncode != 0:
            print(
                f"{Fore.LIGHTRED_EX}[ERROR]{Fore.RESET} git pull exited with return code {git_pull_result.returncode}"
            )
            return False

    except Exception as error:
        print(f"{Fore.LIGHTRED_EX}[ERROR]{Fore.RESET} {error}")
        return False

    finally:
        os.chdir(currdir)

    return True


def get_container_images_for_service(env: str, service_name: str):
    kustomization_path = os.path.join(
        repodir, "k8s", env, service_name, "kustomization.yaml"
    )

    with open(kustomization_path, "r") as f:
        kustomization_data: Dict[str, Any] = yaml.safe_load(f.read())
        images: list[Dict[str, str]] = kustomization_data.get("images", [])

        return images


def get_container_images_map(env: str):
    k8s_dir = os.path.join(repodir, "k8s", env)
    image_map = {}

    for service_name in os.listdir(k8s_dir):
        if os.path.isfile(os.path.join(k8s_dir, service_name)):
            # Omit any files in "k8s/{env}" directory like `kustomization.yaml`
            continue

        images = get_container_images_for_service(env, service_name)

        for image in images:
            *image_name_segments, image_type = image["newName"].split("/")

            image_name = "/".join(image_name_segments)
            image_version = image["newTag"]

            image_map[image_name] = (image_type, image_version)

    return image_map


def get_image_tag(image_type: Optional[str], image_version: Optional[str]):
    if image_version is None or image_type is None:
        return "Container not available"

    return ":".join((image_type, image_version))


def main(
    reference_env: str,
    target_env: str,
    export_csv_filepath: Optional[str] = None,
):
    is_repo_cloned = ensure_repo_cloned()

    if not is_repo_cloned:
        print(
            f"{Fore.LIGHTRED_EX}[ERROR]{Fore.RESET} Exiting due to failure while cloning cxp-deployment"
        )
        exit(1)

    did_update_repo = update_repo()

    if not did_update_repo:
        print(
            f"{Fore.LIGHTRED_EX}[ERROR]{Fore.RESET} Exiting due to failure while pulling latest changes for cxp-deployment"
        )
        exit(1)

    reference_env_image_mappings = get_container_images_map(reference_env)
    target_env_image_mappings = get_container_images_map(target_env)
    reference_env_name = config[reference_env]["name"]
    target_env_name = config[target_env]["name"]

    summarize(
        reference_env_image_mappings,
        target_env_image_mappings,
        reference_env_name,
        target_env_name,
        export_csv_filepath,
    )


if __name__ == "__main__":
    *_, reference_env, target_env = argv
    main(reference_env, target_env)
