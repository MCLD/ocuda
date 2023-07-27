# Ops.Web/css

This folder contains vendor stylesheet files which are updated with the top level `dev/docker-yarn-update.sh` script. When that script is run, the contents of this directory are erased.

If you want to add project-specific JavaScript files please add them in the `Ops.Web/Styles` folder and ensure they are minified in `Startup.cs`.

Files in this directory should be set to copy to the output directory upon build.
