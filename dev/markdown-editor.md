# Markdown editor

To configure the Markdown editor on a page in Ops:

1. Include the Markdown stylesheet:

    ```razor
    @section styles {
        <link rel="stylesheet" href="~/css/md.min.css" asp-append-version="true" />
    }
    ```

2. Create the tag that will contain the editor on the form:

    ```razor
    <div class="mb-3 row">
        <div class="col-md-3 text-md-end">
            <label asp-for="Model.Text" class="col-form-label"></label>
        </div>
        <div class="mb-3-inner col-md-9">
            <textarea asp-for="Model.Text" markdown-editor></textarea>
            <span asp-validation-for="Model.Text"
                class="validation-message text-danger"></span>
        </div>
    </div>
    ```

3. Include the necessary JavaScript to activate the editor:

    ```razor
    @section scripts {
        <script src="~/js/md.min.js" asp-append-version="true"></script>

        <script>
            $(function () {
                var editor = new Markdown.Editor($("#SegmentText_Text"), { allowUploads: false });
                if (editor) {
                    editor.run();
                }
            });
        </script>
    }
    ```
