#!/bin/sh
set -e

PASV_ADDRESS="${PASV_ADDRESS:-127.0.0.1}"
PASV_MIN_PORT="${PASV_MIN_PORT:-21100}"
PASV_MAX_PORT="${PASV_MAX_PORT:-21110}"

sed -e "s/__PASV_ADDRESS__/$PASV_ADDRESS/" \
    -e "s/__PASV_MIN_PORT__/$PASV_MIN_PORT/" \
    -e "s/__PASV_MAX_PORT__/$PASV_MAX_PORT/" \
    /etc/vsftpd.conf.template > /etc/vsftpd.conf

exec vsftpd /etc/vsftpd.conf
