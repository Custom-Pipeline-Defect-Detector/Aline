Background Overview
The solution is an ASP.NET Core Web API that wires up controller endpoints, configures an EF Core DbContext against a SQLite database (kanban.db by default), and exposes uploaded files from a /uploads directory as static content for clients to retrieve.

AppDbContext orchestrates all domain aggregates—users, teams, projects, epics, stories, tasks, sprints, labels, comments, and attachments—defining how they relate to one another (e.g., teams own projects, stories belong to both projects and epics, and attachments can hang off stories, tasks, or comments).

Domain Model Highlights
Users have roles (Admin, Boss, Worker) and can optionally belong to a team, which itself tracks its boss, workers, and projects.

Projects aggregate epics, sprints, and stories, providing helper methods to append to those collections, while epics in turn group their own stories.

Stories capture rich issue-tracking metadata—status, priority, assignee, labels, comments, attachments, and subtasks—and WorkTask entities model the finer-grained tasks with their own attachment lists.

Collaboration artifacts include comments with optional story/task links plus attachment support, alongside reusable color-coded labels and sprint scheduling data to manage agile cadence.

Attachments persist metadata about uploaded files and can be tied to stories, tasks, or comments for traceability across the workspace.

API Surface
StoriesController exposes CRUD-style access patterns: listing stories with assignment and activity counts, fetching detailed records with nested attachments/comments, creating stories, and updating workflow status transitions.

CommentController supports posting new comments and retrieving them with their associated attachments for richer discussions around work items.

AttachmentsController handles file uploads (storing metadata in the database and files on disk) and retrieval of stored assets, ensuring binary resources stay linked to the relevant stories, tasks, or comments.
