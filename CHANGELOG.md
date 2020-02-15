# Change Log
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/).

## Unreleased
### Added
- Pull favicon and similar resources out of the public content path
- Move Promenade Data Protection Key from file system storage to database storage
- Ability to serve out .webmanifest files
- Made logo at top of Promenade clickable to go back to the top of the site
- Response compression
- Configuration and handling of reverse proxy address

### Changed
- Default to only logging Warnings for Microsoft and System namespaces
- Display location feature details pop-ups using Ajax

### Fixed
- Remove hardcoded "Library" from location pages
- Redirects properly bring along query strings

## 1.0.0
### Added
- Everything (initial release)
