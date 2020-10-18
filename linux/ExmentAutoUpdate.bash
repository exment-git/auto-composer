#!/bin/bash
###v1.0.0


# getdate
getdate(){
    os=`uname`
    if [ $os = 'FreeBSD' ]
    then
        dt=`date -j -f "%Y-%m-%dT%H:%M:%S+00:00" "$1" +%s`
    else
        dt=`date --date "$1" +%s`
    fi

    echo $dt
}


# version compare
# https://stackoverflow.com/questions/4023830/how-to-compare-two-strings-in-dot-separated-version-format-in-bash
#0 match
#1 $1 > $2
#-1 $1 < $2
vercomp () {
    if [[ $1 == $2 ]]
    then
        echo 0
        return
    fi
    local IFS=.
    local i ver1=($1) ver2=($2)
    # fill empty fields in ver1 with zeros
    for ((i=${#ver1[@]}; i<${#ver2[@]}; i++))
    do
        ver1[i]=0
    done
    for ((i=0; i<${#ver1[@]}; i++))
    do
        if [[ -z ${ver2[i]} ]]
        then
            # fill empty fields in ver2 with zeros
            ver2[i]=0
        fi
        if ((10#${ver1[i]} > 10#${ver2[i]}))
        then
            echo 1
            return
        fi
        if ((10#${ver1[i]} < 10#${ver2[i]}))
        then
            echo -1
            return
        fi
    done
    echo 0
    return
}

newest () {
    url="https://repo.packagist.org/p/exceedone/exment.json"
    packages=`curl $url | jq -r '.packages["exceedone/exment"]'`
    #echo $packages

    keys=`echo ${packages} | jq -r keys[]`

    newest=""
    lastdatetime=""
    for key in $keys
    do
        items=`echo ${packages} | jq ".[\"${key}\"]"`
        version=`echo ${items} | jq -r '.version'`
        time=`echo ${items} | jq -r '.time'`
        
        if [[ $version =~ ^dev.*$ ]]
        then
            continue
            #echo 'dev'
        fi

        if [[ $lastdatetime = '' ]]
        then
            lastdatetime=$time
            newest=$version
            continue
        fi

        dt1=`getdate $lastdatetime`
        dt2=`getdate $time`
        
        #echo "$dt1 $dt2"

        if [ $dt1 -lt $dt2 ]
        #if [ $dt1 -lt $dt2 ]
        then
            #echo $version
            lastdatetime=$time
            newest=$version
        fi
    done
    
    newest=${newest/v/}
    echo $newest
}

localcomposer () {
    composer=`cat composer.lock | jq -r '.packages[] | select(.name == "exceedone/exment") | .version'`
    echo $composer
}

shellfilename () {
    files=('ExmentUpdateLinuxXserver.sh' 'ExmentUpdateLinuxSakura.sh' 'ExmentUpdateLinux.sh')

    for file in ${files[@]}
    do
        if [ -e $file ]; then
          echo $file
          return
        fi
    done

    echo ''
}

now=`date`
echo "start $now"

newest=`newest`
echo "newest version: $newest"


### Get from composer.lock
composer=`localcomposer`
echo "local composer version: $composer"

if [[ $composer =~ ^dev.*$ ]]
then
  exit
fi

composer=${composer/v/}

comp=`vercomp "$newest" "$composer"`

if [[ $comp > 0 ]]
then
  file=`shellfilename`
  echo "file : $file"
  
  if [ -z "$file" ]; then
    echo 'Bash not found'
    exit
  else
    source $file
    exit
  fi

else
  echo 'No need to call update'
fi
