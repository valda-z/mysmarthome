#! /bin/sh
### BEGIN INIT INFO
# Provides:             grizzlik
# Required-Start:       $local_fs $remote_fs $network $syslog
# Required-Stop:        $local_fs $remote_fs $network $syslog
# Default-Start:        2 3 4 5
# Default-Stop:         0 1 6
# Short-Description: 	grizzlik
### END INIT INFO
 
set -e

PATH=/usr/local/sbin:/usr/local/bin:/sbin:/bin:/usr/sbin:/usr/bin

DESC="smarthome embedded server"
NAME=$0
SCRIPTNAME=/etc/init.d/$NAME
PIDFILE=/run/smarthome.pid
GRDIR=/opt/smarthome


#
#       Function that starts the daemon/service.
#
d_start()
{
    # Starting all processes
    cd $GRDIR
    echo -n ", smarthome"
    if [ -f $PIDFILE ]; then
        echo -n " already running"
    else
       start-stop-daemon --start --quiet \
                       --pidfile $PIDFILE \
                       --chdir $GRDIR \
                       --background -m \
                       --chuid root \
                       --exec ./SmartHomeCore
    fi
}
 
#
#       Function that stops the daemon/service.
#
d_stop() {
    # Killing all frserver processes running
    echo -n ", smarthome"
    start-stop-daemon --stop --quiet --pidfile $PIDFILE \
                          || echo -n " not running"
    if [ -f $PIDFILE ]; then
        rm $PIDFILE
    fi
}
 
ACTION="$1"
case "$ACTION" in
    start)
        echo -n "Starting $DESC: $NAME"
        d_start
        echo "."
        ;;
 
    stop)
        echo -n "Stopping $DESC: $NAME"
        d_stop
        echo "."
        ;;
 
    restart|force-reload)
        echo -n "Restarting $DESC: $NAME"
        d_stop
        sleep 1
        d_start
        echo "."
        ;;
 
    *)
        echo "Usage: $NAME {start|stop|restart|force-reload}" >&2
        exit 3
        ;;
esac
 
exit 0

