from __future__ import annotations

import base64
import json
import uuid
from pathlib import Path

from cryptography.hazmat.primitives.asymmetric import ec


def base64url(data: bytes) -> str:
    return base64.urlsafe_b64encode(data).decode("ascii").rstrip("=")


def find_solution_dir(start: Path | None = None) -> Path:
    start = (start or Path.cwd()).resolve()

    for path in [start, *start.parents]:
        if any(path.glob("*.sln")):
            return path

    raise FileNotFoundError("Could not find a .sln file in the current directory tree.")


def main() -> None:
    solution_dir = find_solution_dir()
    output_dir = solution_dir / "supabase"
    output_dir.mkdir(parents=True, exist_ok=True)

    private_key = ec.generate_private_key(ec.SECP256R1())
    numbers = private_key.private_numbers()
    public_numbers = numbers.public_numbers

    # Convert integers to fixed 32-byte values for P-256
    d = numbers.private_value.to_bytes(32, "big")
    x = public_numbers.x.to_bytes(32, "big")
    y = public_numbers.y.to_bytes(32, "big")

    jwk = {
        "kty": "EC",
        "kid": str(uuid.uuid4()),
        "use": "sig",
        "key_ops": ["sign", "verify"],
        "alg": "ES256",
        "ext": True,
        "d": base64url(d),
        "crv": "P-256",
        "x": base64url(x),
        "y": base64url(y),
    }

    output_path = output_dir / "signing_keys.json"
    output_path.write_text(json.dumps([jwk], indent=2) + "\n", encoding="utf-8")

    print(f"Saved to: {output_path}")


if __name__ == "__main__":
    main()
