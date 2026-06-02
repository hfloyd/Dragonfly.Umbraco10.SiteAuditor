const t = [
  {
    name: "Dragonfly Umbraco 10 SiteAuditor Entrypoint",
    alias: "Dragonfly.SiteAuditor.Entrypoint",
    type: "backofficeEntryPoint",
    js: () => import("./entrypoint-D2Fke-Xb.js")
  }
], i = [
  {
    name: "Dragonfly Site Auditor Dashboard",
    alias: "Dragonfly.SiteAuditor.Dashboard",
    type: "dashboard",
    js: () => import("./dashboard.element-DqXKNLAD.js"),
    meta: {
      label: "Site Auditor",
      pathname: "dragonfly-site-auditor"
    },
    conditions: [
      {
        alias: "Umb.Condition.SectionAlias",
        match: "Umb.Section.Settings"
      }
    ]
  }
], a = [
  ...t,
  ...i
];
export {
  a as manifests
};
//# sourceMappingURL=dragonfly-umbraco-siteauditor.js.map
