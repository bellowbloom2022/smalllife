# Localization Key Naming Rules (Approved)

## Scope
This rule set is approved for current migration and should be used for all new keys.

## Allowed Characters
- lowercase letters: `a-z`
- digits: `0-9`
- underscore: `_`

## Forbidden
- uppercase letters
- hyphen `-`
- spaces
- emoji or non-ASCII symbols in key

## Pattern
- General: `domain_semantic[_seq]`
- Level content: `levelN_semantic[_seq]`

Examples:
- `global_continue`
- `menu_confirm_text`
- `intro_guide_text1`
- `gameplay_control_hint2`
- `level2_guide_3`
- `level3_goal301_description`

## Notes
- Keep keys short but meaningful.
- Remove repeated location fragments (e.g. avoid `level_2_guide_level2_3`).
- Prefer stable semantic names over ad-hoc wording.
- Use `key_map_final.csv` as single source of truth for old->new replacement.
