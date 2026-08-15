# VibeStock In One Page

### What does VibeStock do?
VibeStock is an intelligent data bridge. It takes raw, messy product spreadsheets (CSV/Excel), cleans them up, uses Artificial Intelligence to understand and enhance the product details, and automatically pushes them into a Shopify store.

### Who is it for?
It is for catalog managers and commerce operators who need to bulk-import thousands of products into Shopify but want to ensure the data is accurate, correctly mapped, and optimized for search engines (SEO) without hours of manual data entry.

### What is the main workflow?
1. **Upload:** You upload a spreadsheet.
2. **Map:** The system guesses which columns match Shopify's required fields.
3. **Approve:** You verify and approve the mapping.
4. **Cleanse & Enhance:** The system filters out bad data (like missing SKUs) and uses AI to read your descriptions—automatically extracting tags, fixing tone, and finding SEO gaps.
5. **Sync:** The finalized, enhanced products are safely synced to Shopify.

### Where does AI help?
AI is used in two specific places:
1. **Column Mapping:** Figuring out that a column named "Coste" actually means "Price".
2. **Product Intelligence:** Reading a raw product description and automatically determining its target audience, suggesting SEO improvements, and generating tags. 

### Where does Shopify fit?
Shopify is the final destination. Once VibeStock is completely satisfied that a product is valid and enhanced, it translates the product into Shopify's specific language (GraphQL) and creates the product in the storefront.

### What is automated?
- File parsing and error detection.
- Data Quality checks (e.g., negative price blocking).
- SEO analysis and feature extraction.
- Rate-limit handling and background retries when communicating with Shopify.
- Preventing duplicate uploads if a process is interrupted.

### What requires human approval?
- **AI Column Mapping:** AI will only *suggest* column mappings. A human must explicitly approve them before the data is processed. AI is not allowed to autonomously alter your database schema or column definitions.

### What happens when something fails?
- If a specific product row has bad data, it is flagged as an error, but the rest of the file continues to process smoothly.
- If Shopify is too busy, VibeStock automatically waits and retries.
- If the AI goes offline, VibeStock safely skips the AI enhancement steps or falls back to asking you to manually map columns, ensuring your business keeps running.

### What is ready today?
All core functionality is implemented and fully validated in local testing. This includes file importing, AI mapping, data quality, Product Intelligence, SEO extraction, background job safety (idempotency), and the Shopify integration logic. The system is also heavily optimized for high-performance deployment (NativeAOT).

### What is still pending?
- **External Validation:** We have not yet connected VibeStock to your actual live Shopify store or deployed it to the production Oracle cloud infrastructure.
- **User Interface:** The underlying engine is complete, but the visual front-end screens for operators to click through are not yet built.
