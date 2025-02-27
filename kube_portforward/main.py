from .ui import render


def main(env: str):
    render(env, {"user-api": 8080, "bank-account-api": 8083})
