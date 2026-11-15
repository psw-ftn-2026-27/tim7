// The one place where every analytics event and its parameters are listed. This file is
// platform code: a new event arrives as a pull request the platform team reviews.
//
// Event name: English, lowercase with underscores, the thing and then the action in the past
// tense (tour_published, challenge_solved).
//
// A parameter is an identifier of the thing the event is about, a number, or a value from a
// closed list. Never a name, an email, an account identifier or text the user typed.
export interface AnalyticsEvents {
  tour_published: { tour_id: string };
}

// The role sent with every event as the parameter `user_role`. The Analytics service adds it;
// a module never passes it. 'visitor' is not a system role: it stands for nobody signed in.
export type AnalyticsUserRole = 'visitor' | 'explorer' | 'administrator';
