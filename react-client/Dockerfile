# Build context: react-client/
FROM node:22-alpine AS build
WORKDIR /app

COPY package.json yarn.lock ./
# --ignore-scripts: dependency lifecycle scripts never run (supply-chain hardening, Sonar docker:S6505).
RUN corepack enable && yarn install --frozen-lockfile --ignore-scripts

COPY . .
# Vite inlines VITE_* variables at build time, so the API URL is a build argument.
ARG VITE_API_URL=http://localhost:5094
ENV VITE_API_URL=$VITE_API_URL
RUN yarn build

# Unprivileged nginx runs as a non-root user and listens on 8080 (Sonar docker:S6471).
FROM nginxinc/nginx-unprivileged:1.27-alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 8080
