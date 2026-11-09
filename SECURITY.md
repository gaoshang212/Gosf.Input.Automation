# Security policy

This project is pre-1.0. Security fixes are made on the default development branch; there is no commitment to backport fixes to older versions.

## Reporting a vulnerability

Use the repository's **Security → Report a vulnerability** page for private reports when private vulnerability reporting is enabled. Do not post exploit details, credentials, or sensitive logs in public issues. If private reporting is unavailable, open an issue requesting a private contact channel without disclosing vulnerability details.

Include affected versions, Windows and .NET versions, process bitness and integrity levels, impact, and a minimal reproduction. There is currently no guaranteed response time.

## Scope

DirectInput mode injects native code into another process and installs input hooks. Use it only with applications you are authorized to automate. The library does not bypass Windows permissions or protected-process restrictions. Never pass untrusted window handles or automation commands directly into a privileged process.
