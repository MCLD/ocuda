# Change Log
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/).

## Unreleased
### Added
- Pull favicon and similar resources out of the public content path in Promenade
- Move Promenade Data Protection Key from file system storage to database storage
- Ability to serve out .webmanifest files in Promenade
- Made logo at top of Promenade clickable to go back to the top of the site
- Response compression for Promenade
- Configuration and handling of reverse proxy address in Promenade
- Promenade log enrichment with referer and user agent
- Drop-down capability to Promenade top navigation
- Groups for Promenade emedia content
- Empty robots.txt to avoid 404s when its requested
- Caching of language id lookups in Promenade
- Cache control headers for Promenade
- Promenade.DatabasePoolSize setting for configuring more or less than the default (128)
- Promenade.CachePagesHours setting for how many hours to cache pages in distributed cache
- Promenade.CacheRedirectsHours setting for how many hours to cache redirects in distributed cache
- Add canonical URLs to metadata on pages

### Changed
- Default to only logging Warnings for Microsoft and System namespaces
- Display location feature details pop-ups using Ajax in Promenade
- Text on Promenade emedia page is now segments

### Fixed
- Remove hardcoded "Library" from location pages in Promenade
- Promenade redirects properly bring along query strings
- Ops roster import for vacant roles with no supervisor
- Segment date constraints not working properly
- Cache force refresh of IsTLS site setting

## 1.0.0
### Added
- Everything (initial release)
