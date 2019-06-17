#!/bin/bash


helm install stable/nginx-ingress --namespace kube-system

echo "Waiting for nginx controller to acquire PUBLIC ip ....."
sleep 20
###########
###HTTPS Controller
##########

IP=`kubectl get service -l app=nginx-ingress --namespace kube-system | sed -n '/controller/s/ \+/ /gp' | cut -d" " -f4`

echo "INGRESS NGINX public IP is:"
echo $IP

sed -i.bak s/NGINXIP/$IP/g configure-dns.sh

sed -i.bak1 s/DNSGIVENNAME/$1/g configure-dns.sh

echo "Configuring DNS ...."
sh configure-dns.sh

mv configure-dns.sh.bak configure-dns.sh

rm -rf configure-dns.sh.bak1

echo "Installing cert manager"

helm install stable/cert-manager --set ingressShim.defaultIssuerName=letsencrypt --set ingressShim.defaultIssuerKind=ClusterIssuer

echo "Creating SSL cert issuer in cluster ..."

kubectl apply -f cluster-issuer.yaml

sed -i.bak s/DNSGIVENNAME/$1/g certificates.yaml

sed -i.bak1 s/REGION/$2/g certificates.yaml

mv certificates.yaml.bak certificates.yaml

rm -rf certificates.yaml.bak1

echo "Creating certificates resources ...."

kubectl apply -f certificates.yaml

echo "Creating Remote Monitoring Nginx controller"

sed -i.bak s/DNSGIVENNAME/$1/g rm-ingress.yaml

sed -i.bak s/REGION/$2/g rm-ingress.yaml

kubectl apply -f rm-ingress.yaml

mv rm-ingress.yaml.bak rm-ingress.yaml

rm -rf rm-ingress.yaml.bak1
