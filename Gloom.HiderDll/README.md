# Gloom.HiderDLL

To bypass getting caught by passive malware search, this DLL would be injected to following processes:
* [ ] Explorer.exe (Windows File Explorer) - Hide its folder and files by hijacking file accessing calls
* [ ] taskmgr.exe (Task Manager), tasklist.exe - To hide the main client process from detected by user
* [ ] taskkill.exe - To prevent the client from getting terminated
