# Mock de robots Fanuc (FTP)

Levanta dos servidores FTP (anónimos, solo lectura) que simulan dos robots Fanuc, para probar FanucBCKNET sin hardware real.

Cada uno sirve los mismos archivos de ejemplo (`README.txt`, `SYSVARS.VR`, `PROG1.TP` y `BIGPROG.BIN` de 25 MB, este último para poder ver la barra de progreso avanzar de verdad).

## Arrancar

```bash
cd test/ftp-mock
docker compose up -d --build
```

## Configurar en la app

Crea un proyecto y añade estos dos robots:

| Nombre  | IP        | Puerto |
|---------|-----------|--------|
| Robot 1 | 127.0.0.1 | 2121   |
| Robot 2 | 127.0.0.1 | 2122   |

## Parar

```bash
docker compose down
```
