# FanucBCKNET

Port a .NET + Avalonia de [FanucBCKGUI](https://github.com/nsuarez75/FanucBCKGUI), una aplicación de escritorio para hacer backup por FTP de controladores Fanuc (robots/PLCs).

Mantiene la misma funcionalidad que el original (PyQt5 + pickle):

- Gestión de robots (nombre, grupo, IP), persistidos en `robots.json` junto al ejecutable.
- Filtrado por grupo y selección múltiple de robots.
- Backup por FTP (anónimo) en paralelo hacia una carpeta destino, organizado como `Backup_<grupo>_<ddMMyyyy_HHmm>/<robot>/`.
- Registro de errores por backup en `error.log` dentro de la carpeta del backup.

## Stack

- .NET 10 / C#
- [Avalonia UI](https://avaloniaui.net/) 12 (Fluent theme)
- [FluentFTP](https://github.com/robinrodricks/FluentFTP) 55 para el cliente FTP

## Ejecutar

```bash
dotnet run --project src/FanucBCKNET
```

## Compilar

```bash
dotnet build
```
