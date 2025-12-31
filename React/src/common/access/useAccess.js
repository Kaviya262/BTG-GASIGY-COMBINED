import { useEffect, useState, useCallback } from "react";
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
                    canNew: scr.Save === 1,
                    canPrint: scr.Print === 1,
                    canExport: scr.View === 1,
                    canPost: scr.Post === 1,
                    canViewDetails: scr.ViewDetails === 1,
                    canViewRate: scr.ViewRate === 1,
                    canSendMail: scr.SendMail === 1,
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


    // Disable UI buttons
    const applyAccessUI = useCallback(() => {
        setTimeout(() => {

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
                    btn.disabled = true;
                    btn.style.opacity = "0.5";
                    btn.style.cursor = "not-allowed";
                }
            });

        }, 250);
    }, [access]);

    return { access, applyAccessUI };
}
