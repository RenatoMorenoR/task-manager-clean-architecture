# TaskManager Frontend

This is the React Single Page Application (SPA) for the TaskManager project. It provides a responsive, modern UI built with Vite and TypeScript.

## Architecture & State Management

- **React 18**: Functional components with hooks.
- **Vite**: Fast, modern build tool for development and production bundling.
- **Zustand**: Lightweight state management for handling the authentication state (`authStore`) across the application.
- **Dependency Injection**: The API client interactions are abstracted behind an `ApiContext`, allowing the UI to adhere to SOLID principles (specifically the Dependency Inversion Principle) by not relying on hard-coded HTTP imports.
- **Styling**: Vanilla CSS utilizing modern CSS variables, Flexbox/Grid, and a "Glassmorphism" design system for a premium look and feel.

## Running Locally

To run the frontend against the local backend:

```bash
npm install
npm run dev
```

The application will start on `http://localhost:3000`. Hot Module Replacement (HMR) is enabled.

Ensure you have a `.env.local` file configured to point to the backend API:
```env
VITE_API_URL=http://localhost:5000
```

## Testing

The frontend is fully tested using **Vitest** (a Vite-native test runner) and **React Testing Library**.

```bash
npm test
```

The tests follow Test-Driven Development (TDD) principles, ensuring that components like `TaskForm` and `LoginPage` handle state updates, API mocking, and accessibility requirements correctly.
