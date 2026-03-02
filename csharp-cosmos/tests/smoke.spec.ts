import { test, expect } from "@playwright/test";

test("smoke test: placeholder page is visible", async ({ page }) => {
  await page.goto("/", { waitUntil: "networkidle" });

  await expect(
    page.locator("text=Add your own application code").first()
  ).toBeVisible();
});
