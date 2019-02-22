HTTPS in ASP.NET Core in Docker Linux Containers Deep Dive
==========================================================

This is the demo content for the [HTTPS in ASP.NET Core in Docker Linux Containers Deep Dive](https://robrich.org/slides/https-aspnet-core-docker-deep-dive/#/) presentation.

The demos illustrate:

1. ASP.NET with HTTPS without Docker
2. Add Docker support to the sites, no https
3. Add the build sdk's dev cert to the runtime container
4. Copy the host's dev cert into the runtime container
5. Trust the dev cert
6. A dizzying amount of openssl commands and dockerfiles -- too complex, abort
7. [mkcert](https://github.com/FiloSottile/mkcert) makes this easier
