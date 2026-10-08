# ADR IF-ADR-045 — Recover signed Foundation release from an immutable tag

- Status: Accepted
- Date: 2026-10-08

## Context

Foundation `v0.2.2` is already tagged at `e3fbc5724268f0f909495546a1a98d50449e210d`, but its tag-triggered signed-release workflow failed while verifying the archive. The corrected workflow needs a controlled recovery path without moving or recreating the release tag.

## Decision

Keep tag push as the normal release path. Add a manual recovery path that accepts an explicit tag but currently authorizes only `v0.2.2` at the exact commit above. Resolve the remote tag to its peeled commit, fail closed if it is absent or differs, check out that commit, verify `HEAD` and package identity/version, then sign and validate that exact package content.

Before signing, reject a pre-existing GitHub Release or OpenUPM version. Treat API/registry errors as blocking, so a partial prior publication requires inspection rather than an automatic duplicate attempt. Preserve archive signature and metadata checks. Do not print credentials. Serialize runs per resolved tag. The recovery dispatch must never be run from the release workflow until separately authorized.

## Consequences

The manual path is deliberately limited to this one recovery. A later recovery requires an explicit reviewed change to the authorized tag and commit. Normal tag pushes retain their existing versioned release behavior. This workflow-level validation does not certify Unity import, compilation, or runtime behavior.
