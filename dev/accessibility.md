# Accessibility

Any additions or enhancements to Promenade should include accessibility features.

- Do not duplicate text - if the text is in the link it does not need to be in an `aria-label`
- Sometimes the `title` property is used to describe an item (like an icon without a label), if used then the `aria-label` and `title` properties should not be the same
- If using a `title` property then omit the `aria-label` property
- Do use an `aria-label` containing the text "(opens in a new window)" if that is the case
  - Parenthesis sometimes cause screen readers to pause which is appropriate for this
