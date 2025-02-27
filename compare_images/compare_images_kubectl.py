"""
Usage:
    $ python3 compare_images_kubectl.py [REFERENCE_ENV] [TARGET_ENV]

Example:
    $ python3 compare_images_kubectl.py qa uat
"""

from sys import argv
import subprocess
import os
import json
from typing import Dict, Optional
from colorama import just_fix_windows_console, Fore
from .util import summarize

# Fix colored output in Windows Console (Legacy) by
# preventing ANSI code gibberish from being printed on
# the console and instead get colored output. Might still
# have minor caveats with respect to font styles
just_fix_windows_console()


__dir__ = os.path.dirname(__file__)
config = {}


with open(f"{__dir__}/config.json") as f:
    config = json.loads(f.read())


def fetch_container_images_list(env: str):
    """
    Returns list of fully qualified image names along with image tags (versions)
    in the environment from deployments via kubectl command
    """
    env_config: Dict[str, str] = config[env]
    env_name = env_config["name"]
    env_context = env_config["context"]
    env_namespace = env_config["namespace"]

    info_prefix = f"{Fore.LIGHTBLUE_EX}[INFO]{Fore.RESET}"
    print(f"{info_prefix} Fetching container list for {env_name}")

    container_images_raw = subprocess.getoutput(
        " ".join(
            [
                "kubectl get deployments",
                f'--namespace "{env_namespace}"',
                f'--context "{env_context}"',
                "-o jsonpath=\"{.items[*].spec.template.spec['initContainers', 'containers'][*].image}\"",
            ]
        )
    )

    return container_images_raw.split()


def get_image_maps(env: str):
    """
    Returns a dictionary with key being fully qualified image name
    and value being the corresponding image tag for the given environment
    """

    images = fetch_container_images_list(env)
    image_map = {}

    for image in images:
        *image_path_segments, image_tag = image.split("/")
        image_path = "/".join(image_path_segments)

        image_map[image_path] = image_tag.split(":")

    return image_map


def get_version_from_image_tag(image_tag: str):
    """
    Returns version from image tag (image_type:image_version) by eliminating
    image type

    Example: `candidate:2024-01-04.2` will resolve to `2024-01-04.2`
    """
    return image_tag.split(":")[-1]


def main(
    reference_env: str,
    target_env: str,
    export_csv_filepath: Optional[str] = None,
):
    reference_env_image_mappings = get_image_maps(reference_env)
    target_env_image_mappings = get_image_maps(target_env)
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
