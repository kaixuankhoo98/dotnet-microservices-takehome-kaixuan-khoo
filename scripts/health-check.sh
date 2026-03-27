#!/usr/bin/env bash
set -euo pipefail

CONTAINERS=(
  "api-gateway"
  "order-service"
  "payment-service"
  "notification-service"
  "rabbitmq"
  "sqlserver"
  "seq"
)

API_CHECKS=(
  "notification-service:5030"
  "order-service:5211"
  "payment-service:5104"
  "api-gateway:5000"
)

any_fail=0

echo "=== Container health checks ==="
for name in "${CONTAINERS[@]}"; do
  if status="$(docker inspect --format '{{if .State.Health}}{{.State.Health.Status}}{{else}}{{.State.Status}}{{end}}' "$name" 2>/dev/null)"; then
    if [[ "$status" == "healthy" || "$status" == "running" ]]; then
      echo "PASS  $name ($status)"
    else
      echo "FAIL  $name ($status)"
      any_fail=1
    fi
  else
    echo "FAIL  $name (not found)"
    any_fail=1
  fi
done

echo
echo "=== API health endpoints ==="
for check in "${API_CHECKS[@]}"; do
  service="${check%%:*}"
  port="${check##*:}"
  url="http://localhost:${port}/health"
  code="$(curl -s -o /dev/null -w "%{http_code}" "$url" || echo "000")"

  if [[ "$code" == "200" ]]; then
    echo "PASS  $service $url ($code)"
  else
    echo "FAIL  $service $url ($code)"
    any_fail=1
  fi
done

echo
if [[ $any_fail -eq 0 ]]; then
  echo "All checks passed."
  exit 0
fi

echo "One or more checks failed."
exit 1
