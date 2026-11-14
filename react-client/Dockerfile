# Build context: react-client/
FROM node:22-alpine AS build
WORKDIR /app

COPY package.json yarn.lock ./
RUN corepack enable && yarn install --frozen-lockfile

COPY . .
# Vite inlines VITE_* variables at build time, so the API URL is a build argument.
ARG VITE_API_URL=http://localhost:5094
ENV VITE_API_URL=$VITE_API_URL
RUN yarn build

FROM nginx:1.27-alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
