export const manifests: Array<UmbExtensionManifest> = [
  {
    name: "Dragonfly Site Auditor Dashboard",
    alias: "Dragonfly.SiteAuditor.Dashboard",
    type: "dashboard",
    js: () => import("./dashboard.element.js"),
    meta: {
      label: "Site Auditor",
      pathname: "dragonfly-site-auditor",
    },
    conditions: [
      {
        alias: "Umb.Condition.SectionAlias",
        match: "Umb.Section.Settings",
      },
    ],
  },
];
