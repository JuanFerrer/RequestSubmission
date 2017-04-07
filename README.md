# Request Submission

This program is intended to be a simple and quick program to submit and solve requests in a shared environment (e.g. request to admin for program installation, request to user to reduce folder size, etc.).

The formatted text is passed through [encrypt.exe](https://github.com/JuanFerrer/Encryption) and saved in rfc.db. While the program is open, an unencrypted but hidden version of the text is held in temp.db.

No security, other than encryption, is implemented yet. The file can still be deleted or modified manually, which can corrupt the file. Probably a backup somewhere else in disk should be saved in case the local copy is corrupted.