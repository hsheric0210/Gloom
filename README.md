# Gloom - A very simple and practical Backdoor

Educational purpose. **Do not execute client on your computor without any protection.** Use at your own risk.

Use VM(VirtualBox, VMware, etc.) or Sandbox(Sandboxie, etc.) to execute the client.

* [x] Integrated C&C server and communicating protocol

## Features
* [x] Process list collector
* [x] Environment variable collector
* [ ] File uploader / downloader
* [ ] Remote file executor
* [ ] KeyLogger
* [ ] Clipboard Logger
* [ ] Screenshot Capturer
* [ ] Remote DLL Injector
* [ ] Remote Code Execution (Upload executable and execute remotely / Compile-and-Execute C# or VisualBasic.NET code with CodeDom)
* [ ] Remote Process Terminator / Memory Dumper
* [ ] Client Updater
* [ ] ZipBomb

## Communication
* [x] Fully encrypted communication between server and client -> Using RSA-8192 as key agreement algorithm, AES-256 as message encryption algorithm.
* [ ] Periodic key re-generating

## Stealth
* [ ] Self-replicate to random folder when executed
* [ ] Register itself on Task Scheduler, Registry Autorun, etc.
* [ ] When remote-code-execution, bypass getting detected by unpacking executables in _ENCRYPTED_ form
* [ ] Store some strings (such as discord token stealer regex, etc.) in encrypted form to bypass getting detected by resource analysis
