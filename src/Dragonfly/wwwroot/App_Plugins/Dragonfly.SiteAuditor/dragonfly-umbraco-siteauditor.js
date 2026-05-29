const a = [
  {
    name: "Dragonfly Umbraco 10 SiteAuditor Entrypoint",
    alias: "Dragonfly.SiteAuditor.Entrypoint",
    type: "backofficeEntryPoint",
    js: () => import("./entrypoint-CK4R624y.js")
  }
], t = [
  {
    name: "Dragonfly Umbraco 10 SiteAuditor Dashboard",
    alias: "Dragonfly.SiteAuditor.Dashboard",
    type: "dashboard",
    js: () => import("./dashboard.element-Cdm0vhRT.js"),
    meta: {
      label: "Example Dashboard",
      pathname: "example-dashboard"
    },
    conditions: [
      {
        alias: "Umb.Condition.SectionAlias",
        match: "Umb.Section.Content"
      }
    ]
  }
], o = [
  ...a,
  ...t
];
export {
  o as manifests
};
//# sourceMappingURL=dragonfly-umbraco-siteauditor.js.map
