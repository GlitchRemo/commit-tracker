import csv
from typing import Dict, Optional, Tuple
from rich.console import Console

console = Console()
ImageMapping = Dict[str, Tuple[str, str]]


def get_image_tag(
    image_type: Optional[str], image_version: Optional[str], markup: bool = True
):
    NO_CONTAINER = "Container not available"

    if image_version is None or image_type is None:
        return NO_CONTAINER if not markup else f"[bright_black]{NO_CONTAINER}[/]"

    if not markup:
        return ":".join((image_type, image_version))
    else:
        return ":".join(
            (
                f"[bright_magenta italic]{image_type}[/]",
                f"[cyan italic]{image_version}[/]",
            )
        )


def write_csv_to_file(filename: str, data: list[Dict[str, str]]):
    header = data[0].keys()

    with open(filename, "w") as f:
        dict_writer = csv.DictWriter(f, header)
        dict_writer.writeheader()
        dict_writer.writerows(data)


def summarize(
    reference_env_image_mappings: ImageMapping,
    target_env_image_mappings: ImageMapping,
    reference_env_name: str,
    target_env_name: str,
    export_csv_filepath: Optional[str] = None,
):
    data = []
    unmatched_count = 0
    target_env_no_container_count = 0

    for image_name in sorted(reference_env_image_mappings):
        reference_env_image_tag = reference_env_image_mappings.get(
            image_name, (None, None)
        )
        target_environment_image_tag = target_env_image_mappings.get(
            image_name, (None, None)
        )

        if target_environment_image_tag[1] is None:
            target_env_no_container_count += 1

        reference_env_image_version = reference_env_image_tag[1]
        target_env_image_version = target_environment_image_tag[1]
        did_match = reference_env_image_version == target_env_image_version

        if did_match:
            console.print(
                "[green bold][✓][/]",
                image_name,
                f"[blue bold]({reference_env_image_version})[/]",
                highlight=False,
            )

        else:
            unmatched_count += 1
            is_container_avilable = target_environment_image_tag[1] is not None

            console.print(
                "[yellow bold][ ][/]",
                image_name,
                "[red bold](unmatched image versions)[/]",
                highlight=False,
            )

            console.print(
                f"\t[white bold]{reference_env_name} Image Tag:[/]",
                get_image_tag(*reference_env_image_tag),
                highlight=False,
            )

            console.print(
                f"\t[white bold]{target_env_name} Image Tag:[/]",
                get_image_tag(*target_environment_image_tag),
                highlight=False,
            )

        if export_csv_filepath is not None:
            data.append(
                {
                    "Image Name": image_name,
                    reference_env_name: get_image_tag(*reference_env_image_tag),
                    target_env_name: get_image_tag(*target_environment_image_tag),
                }
            )

    console.print("\n[white bold]SUMMARY:[/]")
    console.print(
        "\t[blue]:blue_square:[/]",
        "Total containers processed =",
        len(reference_env_image_mappings),
    )
    console.print(
        "\t[green]:green_square:[/]",
        "Total containers with same image versions =",
        len(reference_env_image_mappings) - unmatched_count,
    )
    console.print(
        "\t[yellow]:yellow_square:[/]",
        "Total containers with mismatched image versions =",
        unmatched_count,
    )
    console.print(
        "\t[yellow]:yellow_square:[/]",
        f"Total containers not available in {target_env_name} =",
        target_env_no_container_count,
    )

    if export_csv_filepath is not None:
        write_csv_to_file(export_csv_filepath, data)
