# Build Stage
FROM node:22-alpine AS build
WORKDIR /app

# Copy package files
COPY web_dashboard/package.json web_dashboard/package-lock.json ./
RUN npm ci

# Copy application source
COPY web_dashboard/ .

# Build the React application using Vite
ARG VITE_API_URL
ENV VITE_API_URL=$VITE_API_URL
RUN npm run build

# Final Production Image using Nginx
FROM nginx:alpine AS final

# Copy Nginx configuration
COPY web_dashboard/nginx.conf /etc/nginx/templates/default.conf.template

# Copy build artifacts to Nginx html folder
COPY --from=build /app/dist /usr/share/nginx/html


CMD ["nginx", "-g", "daemon off;"]
