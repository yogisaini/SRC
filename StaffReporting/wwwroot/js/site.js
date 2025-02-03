$('#filterFormPanel').hide();
$(document).on('click', '#toggleFormButton', function () {
    $('#filterFormPanel').toggle(); // Toggles visibility of the filter form

    // Change the icon class based on visibility
    const isVisible = $('#filterFormPanel').is(':visible');
    const icon = $(this).find('i'); // Select the icon inside the button

    if (isVisible) {
        icon.removeClass('fa fa-eye').addClass('fa fa-eye-slash'); // Icon for "Hide"
    } else {
        icon.removeClass('fa fa-eye-slash').addClass('fa fa-eye'); // Icon for "Show"
    }
});

$(document).on('click', '.assign-user-btn', function () {
    var workId = $(this).data("work-id");

    $.ajax({
        url: "/AssignUser/ProjectAssignUser",
        type: "GET",
        data: { Id: workId },
        success: function (data) {
            $("#assignUserModalContainer").html(data);
            $("#commentsModal1").modal("show");
        },
        error: function () {
            alert("Error loading assigned users.");
        }
    });
});

$(document).on('click', '.add-comment', function () {
    const taskId = $(this).data('task-id');
    const commentText = $(`.commentText-${taskId}`).val();

    if (commentText === '' || commentText === '<p><br></p>') {
        $(`.commentText-${taskId}`).addClass('is-invalid');
        $(`.commentText-${taskId}`).after('<div class="invalid-feedback">Comment cannot be empty!</div>');
        return;
    } else {
        $(`.commentText-${taskId}`).removeClass('is-invalid');
        $(`.commentText-${taskId}`).next('.invalid-feedback').remove();
    }

    $.ajax({
        url: '/Account/AddComment',
        type: 'POST',
        data: {
            TaskId: taskId,
            Comment: commentText
        },
        success: function (response) {
            if (response.success) {
                $(`#commentModal-${taskId}`).modal('hide');
                $(`#comments-section-${taskId}`).html(response.html);
                $(`.commentText-${taskId}`).val('');
            } else {
                alert('Failed to add comment. Please try again.');
            }
        },
        error: function () {
            alert('An error occurred. Please try again.');
        }
    });
});

$(function () {
    $.ajax({
        url: "/Notifications/GetNotifications",
        type: "GET",
        success: function (data) {
            $("#notificationsContainer").html(data);
        },
        error: function (xhr, status, error) {
            console.error("Error loading notifications:", error);
        }
    });
});

$(function () {
    $('.summernote').summernote({
        width: '100%',
        height: 100, // Set default height
        maxHeight: 500, // Set maximum height
        focus: true, // Focus on editor when initialized
        placeholder: 'Start typing here...', // Add a placeholder
        disableDragAndDrop: true, // Disable drag & drop to prevent unwanted behavior
        toolbar: [
            ['style', ['bold', 'italic', 'underline', 'clear']],
            ['font', ['superscript', 'subscript']],
            ['para', ['ul', 'ol', 'hr']],
            ['color', ['color']]
        ]
    });
});

$(function () {
    // Initialize Select2
    $('.select-user').select2({
        theme: 'bootstrap-5',
        placeholder: "Select user",
        allowClear: true,
        width: '100%'
    });

    $('.select-work').select2({
        theme: 'bootstrap-5',
        placeholder: "Select project",
        allowClear: true,
        width: '100%'
    });

    $('.select-Dept').select2({
        theme: 'bootstrap-5',
        placeholder: "Select Department",
        allowClear: true,
        width: '100%'
    });

    $('.select-Task').select2({
        theme: 'bootstrap-5',
        placeholder: "Select Task",
        allowClear: true,
        width: '100%'
    });
    // Initialize DataTables
    $('.table').DataTable({
        responsive: true,
        pageLength: 50
    });

});