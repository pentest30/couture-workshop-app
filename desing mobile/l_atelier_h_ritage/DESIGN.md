# Design System: The Artisan’s Digital Thread

## 1. Overview & Creative North Star: "The Digital Atelier"
This design system is not a utility; it is an extension of the craft. Our Creative North Star is **"The Digital Atelier"**—a space where the precision of a needle meets the fluidity of silk. We move away from the rigid, boxy layouts of standard SaaS and embrace an editorial, high-fashion layout that mirrors the experience of a private tailoring consultation.

To achieve this, we break the "template" look using:
*   **Intentional Asymmetry:** Hero elements and image clusters should never be perfectly centered. Use the `Spacing Scale` to create rhythmic, off-balance compositions that feel curated.
*   **Layered Textures:** Elements should feel like layered fabric. We use surface shifts and soft gradients rather than hard borders to define space.
*   **Atmospheric Breathing Room:** High-end luxury is defined by what *isn't* there. Use aggressive white space (`spacing.16` to `spacing.24`) to separate major conceptual sections.

---

## 2. Colors: A Palette of Heritage and Depth
The color strategy is designed to evoke the warmth of an Algerian workshop—terracotta-adjacent golds, deep royal plums, and the crispness of fresh linen.

### The Palette
*   **Primary (Deep Plum):** `#410050` (Primary) to `#5C1A6B` (Container). This is our signature. Use it for high-impact moments and navigation headers.
*   **Secondary (Warm Gold):** `#7F5700`. Reserved for "The Golden Thread"—subtle accents, active states, and refined dividers.
*   **Neutral (The Canvas):** `surface_container_lowest` (#FFFFFF) and `background` (#FCF9F4).

### The "No-Line" Rule
**Strict Mandate:** Prohibit 1px solid black or high-contrast borders for sectioning. 
Structure must be defined through:
1.  **Background Color Shifts:** Place a `surface_container_low` card on a `background` page.
2.  **Tonal Transitions:** Use the `outline_variant` at 15% opacity only if a physical boundary is required for accessibility.

### Signature Textures & Glassmorphism
To add "soul," use a soft gradient for headers: `linear-gradient(135deg, #410050 0%, #5C1A6B 100%)`. For floating modals or navigation bars, apply **Glassmorphism**: 
*   `Background: rgba(252, 249, 244, 0.8)` 
*   `Backdrop-filter: blur(12px)`
*   This ensures the UI feels like a sheer veil over the content.

---

## 3. Typography: Editorial Authority
We pair the high-contrast elegance of **Noto Serif** (Playfair Display equivalent) with the functional clarity of **Manrope** (DM Sans equivalent).

*   **Display (Large/Med):** `notoSerif`, `3.5rem` / `2.75rem`. Use for primary branding and artisan showcase headers. Letter spacing should be slightly tight (-0.02em) to feel "tucked."
*   **Headlines:** `notoSerif`, `2rem`. Used for page titles. Treat these as headlines in a fashion magazine.
*   **Title/Body:** `manrope`, `1.125rem` to `1rem`. Manrope provides a modern, technical contrast to the serif headings, signifying the "professionalism" of the workshop.
*   **Labels:** `manrope`, `0.75rem`, `Uppercase`, `Letter-spacing: 0.1em`. Used for "Status" and "Metadata" to create a "tag" or "label" aesthetic.

---

## 4. Elevation & Depth: Tonal Layering
Traditional drop shadows are too "digital." We use **Tonal Layering** to create a sense of physical objects resting on a table.

*   **The Layering Principle:** 
    *   Base Page: `background` (#FCF9F4)
    *   Main Content Cards: `surface_container_lowest` (#FFFFFF)
    *   Embedded Details: `surface_container` (#F0EDE9)
*   **Ambient Shadows:** For "Floating" elements (e.g., a 'New Order' FAB), use a shadow tinted with the primary color: `box-shadow: 0 12px 32px rgba(65, 0, 80, 0.06);`.
*   **The Ghost Border:** If separation is needed on white-on-white, use a `1px` border of `outline_variant` (#D2C2CF) at **20% opacity**. It should be felt, not seen.

---

## 5. Components: The Artisan’s Toolkit

### Buttons
*   **Primary:** Solid `primary` (#410050), `rounded-full`, with a `gold` hover state.
*   **Secondary (The Gold Stitch):** Transparent background, `outline` in `secondary` (#7F5700), `rounded-full`.
*   **Tertiary:** Text-only, `manrope` bold, uppercase, with a 2px `secondary` underline that expands on hover.

### Status Badges (Pills)
Status badges must be pill-shaped (`rounded-full`) using a soft "Wash" of the status color (15% opacity) with high-contrast text.
*   **Embroidery:** Deep Purple Wash / Deep Purple Text.
*   **Beading:** Mauve Wash / Mauve Text.
*   **Ready:** Green Wash / Green Text.

### Cards & Lists
*   **Radius:** `rounded-lg` (1rem / 16px) for main containers; `rounded-md` (0.75rem / 12px) for inner elements.
*   **The Divider Rule:** Forbid standard grey dividers. Use a "Gold Thread": a 1px line using `secondary_fixed_dim` (#F6BD5E) that does not span the full width of the card—leave 24px padding on either side.

### Special Component: The Process Timeline
Given the workshop nature, use a vertical "Stitch Line" (dashed `secondary` line) to connect order phases, with needle-head icons as nodes.

---

## 6. Do’s and Don’ts

### Do:
*   **Do** use asymmetrical margins. A photo of a garment should sit slightly "off" the grid to look editorial.
*   **Do** use `notoSerif` for numbers in a pricing or measurement context to emphasize the "custom" nature.
*   **Do** use the `surface_bright` token for active hover states on cards to create a "lit from within" effect.

### Don’t:
*   **Don’t** use pure black (#000000). Always use `on_surface` (#1C1C19) for text to maintain the "Warm Professional" aesthetic.
*   **Don’t** use heavy drop shadows. If an element looks like it’s "hovering" more than 4px off the page, it’s too high.
*   **Don’t** use standard "Checkmark" icons for completion. Use a "Cross-stitch" or "Needle & Thread" motif to reinforce the brand identity.
*   **Don't** use 100% opaque dividers. They "cut" the fabric of the UI; we want it to feel continuous.