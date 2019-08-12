call npm uninstall -g iot-solutions
call npm ci
call npm start
call npm link
call pcs login 
call pcs -t remotemonitoring -s basic -r dotnet
