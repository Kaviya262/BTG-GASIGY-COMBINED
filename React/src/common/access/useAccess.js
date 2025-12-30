import { useEffect, useState, useCallback, useRef } from "react";
import { GetUserAccess } from "../../common/data/mastersapi";

export default function useAccess(moduleName, screenName) {

    const [access, setAccess] = useState({
        canView: false,
        canEdit: false,
        canDelete: false,
        canSave: false,
        canNew: false,
        canPrint: false,
        canExport: false,
        canPost: false,
        canViewDetails: false,
        canViewRate: false,
        canSendMail: false,
        loading: true
    });

    useEffect(() => {
        const fetchAccess = async () => {
            try {
                const auth = localStorage.getItem("authUser");
                const userId = auth ? JSON.parse(auth).u_id : null;

                if (!userId) {
                    setAccess({
                        canView: false, canEdit: false, canDelete: false,
                        canSave: false, canNew: false, canPrint: false,
                        canExport: false, canPost: false, canViewDetails: false,
                        loading: false, canViewRate: false, canSendMail: false
                    });
                    return;
                }

                const result = await GetUserAccess(userId);
                const list = result?.data || [];

                // FIX: FIND EXACT RECORD FOR MODULE + SCREEN
                const scr = list.find(
                    x =>
                        x.Module?.toLowerCase() === moduleName.toLowerCase() &&
                        x.Screen?.toLowerCase() === screenName.toLowerCase()
                );

                // If record NO FOUND = no access at all
                if (!scr) {
                    setAccess({
                        canView: false, canEdit: false, canDelete: false,
                        canSave: false, canNew: false, canPrint: false,
                        canExport: false, canPost: false, canViewDetails: false,
                        loading: false, canViewRate: false, canSendMail: false
                    });
                    return;
                }

                //  If View = 0 → Disable everything
                if (scr.View === 0) {
                    setAccess({
                        canView: false, canEdit: false, canDelete: false,
                        canSave: false, canNew: false, canPrint: false,
                        canExport: false, canPost: false, canViewDetails: false,
                        loading: false, canViewRate: false, canSendMail: false
                    });
                    return;
                }

                // Otherwise read DB values
                setAccess({
                    canView: scr.View === 1,
                    canEdit: scr.Edit === 1,
                    canDelete: scr.Delete === 1,
                    canSave: scr.Save === 1,
                    canNew: scr.New === 1,
                    canPrint: scr.Print === 1,
                    canExport: scr.View === 1,
                    canPost: scr.Post === 1,
                    canViewDetails: scr.ViewDetails === 1,
                    canViewRate: scr.ViewRate === 1,
                    canSendMail: scr.SendMail === 1,
                    records: scr.Records || 10,
                    loading: false
                });

            } catch (err) {
                console.error("Access error:", err);
                setAccess({
                    canView: false, canEdit: false, canDelete: false,
                    canSave: false, canNew: false, canPrint: false,
                    canExport: false, canPost: false, canViewDetails: false,
                    loading: false, canViewRate: false, canSendMail: false
                });
            }
        };

        fetchAccess();
    }, [moduleName, screenName]);


    const hideTimerRef = useRef();

    // Hide / show UI buttons based on current access
    const applyAccessUI = useCallback(() => {
        if (access.loading || typeof document === "undefined") {
            return;
        }

        if (hideTimerRef.current) {
            clearTimeout(hideTimerRef.current);
        }

        hideTimerRef.current = setTimeout(() => {
            const buttons = document.querySelectorAll("button[data-access]");

            buttons.forEach(btn => {
                const key = btn.getAttribute("data-access")?.toLowerCase();

                const allow = {
                    view: access.canView,
                    edit: access.canEdit,
                    delete: access.canDelete,
                    save: access.canSave,
                    new: access.canNew,
                    print: access.canPrint,
                    export: access.canExport,
                    post: access.canPost,
                    viewdetails: access.canViewDetails,
                    viewrate: access.canViewRate,
                    sendmail: access.canSendMail
                };

                if (!allow[key]) {
                    btn.style.display = "none";
                } else {
                    btn.style.display = "inline-flex";
                }
            });
        }, 200);
    }, [access]);

    useEffect(() => {
        return () => {
            if (hideTimerRef.current) {
                clearTimeout(hideTimerRef.current);
            }
        };
    }, []);

    // Auto-apply whenever access state settles
    useEffect(() => {
        if (!access.loading) {
            applyAccessUI();
        }
    }, [access, applyAccessUI]);

    // Re-apply whenever new nodes are injected into DOM (e.g. popups/modals)
    useEffect(() => {
        if (access.loading || typeof MutationObserver === "undefined" || typeof document === "undefined") {
            return;
        }

        const observer = new MutationObserver(() => {
            applyAccessUI();
        });

        observer.observe(document.body, { childList: true, subtree: true });

        return () => observer.disconnect();
    }, [access.loading, applyAccessUI]);


    return { access, applyAccessUI };
}