#!/bin/bash

SCRIPTDIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )

# Get process PID
getpid(){
	#echo "getpid args: " $1
	TMPPID=""
	TMPPID=`ps -eo pid,args|grep "$1"|grep -v "grep $1"|sed -r -n -e 's/^ {0,5}([0-9]{1,9}) (.*)/\1/p'`
	#echo "getpid: " $TMPPID
}

# configuration
FILEHOSTAPDCONF=/etc/hostapd/hostapd.conf
CMDHOSTAPD="/usr/sbin/hostapd -B /etc/hostapd/hostapd.conf"
CMDUDHCPD="/usr/sbin/udhcpd -S /etc/udhcpd.conf"
APDNAME=grizzly2G
APDPSK=monika2410

# create hostapd config file
echo "#Hostapd configuration file" > $FILEHOSTAPDCONF
echo "interface=wlan0_ap" >> $FILEHOSTAPDCONF
echo "driver=nl80211" >> $FILEHOSTAPDCONF
echo "ssid="$APDNAME >> $FILEHOSTAPDCONF
echo "hw_mode=g" >> $FILEHOSTAPDCONF
echo "channel=9" >> $FILEHOSTAPDCONF
echo "macaddr_acl=0" >> $FILEHOSTAPDCONF
echo "auth_algs=1" >> $FILEHOSTAPDCONF
echo "ignore_broadcast_ssid=0" >> $FILEHOSTAPDCONF
echo "wpa=2" >> $FILEHOSTAPDCONF
echo "wpa_passphrase="$APDPSK >> $FILEHOSTAPDCONF
echo "wpa_key_mgmt=WPA-PSK" >> $FILEHOSTAPDCONF
echo "wpa_pairwise=TKIP" >> $FILEHOSTAPDCONF
echo "rsn_pairwise=CCMP" >> $FILEHOSTAPDCONF

# infinite loop checking channel config
while true; do

	WLAN0NAME=`iwconfig wlan0 | grep wlan0`
	if [ -n "$WLAN0NAME" ]
	then

		# startup processes if necessary
		getpid "$CMDHOSTAPD"
		if [ -z "$TMPPID" ]
		then
			echo "START: " $CMDHOSTAPD
			$CMDHOSTAPD
		fi
		getpid "$CMDUDHCPD"
		if [ -z "$TMPPID" ]
		then
			ifup wlan0_ap
			echo "START: " $CMDUDHCPD
			$CMDUDHCPD
		fi

		HOSTAPDCHANNEL=`cat $FILEHOSTAPDCONF | grep "channel=" | sed -r -n -e 's/^channel=([0-9]{1,3})/\1/p'`

		CHANNEL=`iwlist wlan0 channel | grep "Current Frequency" | sed -r -n -e 's/^.*Current Frequency.*Channel ([0-9]{1,3})\).*/\1/p'`

		if [ -z "$CHANNEL" ]
		then
			CHANNEL=9
		fi

		echo CHANNEL: $CHANNEL
		echo HOSTAPDCHANNEL: $HOSTAPDCHANNEL
		if [ "$CHANNEL" != "$HOSTAPDCHANNEL" ]
		then
			echo "Restart of hostapd needed..."
			cat $FILEHOSTAPDCONF > $FILEHOSTAPDCONF.bak
			sed "/^channel/s/\(.[^=]*\)\([ \t]*=[ \t]*\)\(.[^=]*\)/\1\2$CHANNEL/" $FILEHOSTAPDCONF.bak > $FILEHOSTAPDCONF
			/usr/bin/killall hostapd
			# start hostapd daemon
			/usr/sbin/hostapd -B $FILEHOSTAPDCONF
		fi
	else
		# kill processes ...
		getpid "$CMDHOSTAPD"
		if [ -n "$TMPPID" ]
		then
			echo "KILL: " $CMDHOSTAPD
			kill $TMPPID			
		fi
		getpid "$CMDUDHCPD"
		if [ -n "$TMPPID" ]
		then
			echo "KILL: " $CMDUDHCPD
			kill $TMPPID			
		fi
	fi

	sleep 6
done

