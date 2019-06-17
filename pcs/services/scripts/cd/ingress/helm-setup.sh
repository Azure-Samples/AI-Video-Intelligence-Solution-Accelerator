#!/bin/bash


kubectl create -f helm-rbac.yaml

helm init --service-account tiller

