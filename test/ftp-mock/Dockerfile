FROM alpine:3.20

RUN apk add --no-cache vsftpd

RUN (adduser -D -H -h /ftp/anonymous -s /sbin/nologin ftp || true) && \
    mkdir -p /ftp/anonymous && \
    echo "Backup simulado generado por el entorno de pruebas de FanucBCKNET" > /ftp/anonymous/README.txt && \
    echo "GRP1:UTOOL[1]=1 UFRAME[1]=1" > /ftp/anonymous/SYSVARS.VR && \
    echo "1 J P[1] 100% FINE" > /ftp/anonymous/PROG1.TP && \
    dd if=/dev/urandom of=/ftp/anonymous/BIGPROG.BIN bs=1M count=25 2>/dev/null && \
    chown -R root:root /ftp/anonymous && \
    chmod -R a+rX /ftp/anonymous

COPY vsftpd.conf.template /etc/vsftpd.conf.template
COPY entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

EXPOSE 21
ENTRYPOINT ["/entrypoint.sh"]
