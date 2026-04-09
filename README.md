# VGC College Management System

ASP.NET Core MVC application for CA3, built with .NET 9, Entity Framework Core, SQLite, Identity, xUnit, and GitHub Actions.

## Features

- Branch management
- Course management
- Student profile management
- Faculty profile seed support
- Course enrolment management
- Attendance tracking
- Assignment management
- Assignment results / gradebook
- Exam management
- Exam results with release control
- Role-based access control for:
  - Administrator
  - Faculty
  - Student

## Student Access Rules

Students can only view:
- their own profile
- their own enrolments
- their own attendance
- their own assignment results
- their own exam results

Students cannot view exam results unless the related exam has `ResultsReleased = true`.

## Tech Stack

- ASP.NET Core MVC (.NET 9)
- Entity Framework Core
- SQLite
- ASP.NET Core Identity
- Serilog
- xUnit

## Project Structure

- `src/VgcCollege.Web` - main MVC application
- `tests/VgcCollege.Tests` - unit tests

## Seeded Users

- Administrator  
  - Email: `admin@vgc.com`
  - Password: `123456`

- Faculty  
  - Email: `faculty@vgc.com`
  - Password: `123456`

- Student 1  
  - Email: `student1@vgc.com`
  - Password: `123456`

- Student 2  
  - Email: `student2@vgc.com`
  - Password: `123456`

## Business Rules Implemented

- A student cannot be enrolled twice in the same course.
- A student cannot have overlapping active enrolments in different courses during the same period.
- Attendance cannot be duplicated for the same enrolment in the same week.
- Assignment results cannot be duplicated for the same student and assignment.
- Exam results cannot be duplicated for the same student and exam.
- Students can only see exam results when `ResultsReleased = true`.

## How to Run

From the project root:

```bash
dotnet restore
dotnet build
cd .\src\VgcCollege.Web
dotnet run
````

## How to Run Tests

From the solution root:

```bash
dotnet test
```

## CI

GitHub Actions is included to automatically:

* restore
* build
* test

on every push and pull request.

## Notes

This application uses SQLite for local persistence and `EnsureCreated()` during startup seed initialization.
