import PropTypes from "prop-types";
import React, { Component } from "react";
import SimpleBar from "simplebar-react";
import MetisMenu from "metismenujs";
import { withRouter } from "react-router-dom";
import { Link } from "react-router-dom";
import { withTranslation } from "react-i18next";
import { GetMenuDetails } from "common/data/mastersapi";

class SidebarContent extends Component {
    constructor(props) {
        super(props);
        this.refDiv = React.createRef();
    }

    componentDidMount = async () => {
        // debugger;
        this.loadMenuDetails();
        // this.initMenu();
    }

    state = {
        dynamicMenu: { menus: [] }
    };

    loadMenuDetails = async () => {
        // 1. Load existing data
        let menuData = JSON.parse(localStorage.getItem("userMenu"));

        // Safety check if localstorage is empty
        if (!menuData) {
            menuData = { menus: [] };
        }

        // ---------------------------------------------------------
        // 2. INJECT BANK BOOK ENTRIES INTO FINANCE
        // ---------------------------------------------------------

        // Find the Finance module (case-sensitive)
        const financeModule = menuData.menus.find(m => m.moduleName === "Finance");

        if (financeModule) {
            // Check if it already exists to prevent duplicates on re-renders
            const alreadyExists = financeModule.screen.find(s => s.screenName === "Bank Book Entries");

            if (!alreadyExists) {
                financeModule.screen.push({
                    screenId: 99902, // Temporary unique ID
                    screenName: "Bank Book Entries",
                    url: "/bank-book-entries", // Placeholder URL
                    icon: "bx bx-book", // The Book Icon
                    module: [] // No sub-menus
                });
            }
        } else {
            console.warn("Finance module not found in menuData");
        }

        // ---------------------------------------------------------
        // 3. INJECT 'AR BOOK DO' INTO REPORTS (NEW LOGIC)
        // ---------------------------------------------------------
        const reportsModule = menuData.menus.find(m => m.moduleName === "Reports");

        if (reportsModule) {
            // Check if it already exists to prevent duplicates
            const arBookDoExists = reportsModule.screen.find(s => s.screenName === "AR Book DO");

            if (!arBookDoExists) {
                // Find the index of "AR Book" to insert right after it
                const arBookIndex = reportsModule.screen.findIndex(s => s.screenName === "AR Book");

                const newArBookDo = {
                    screenId: 99903, // Unique ID for the new menu
                    screenName: "AR Book DO",
                    url: "/ar-book-do", // Ensure you have this route defined in your App.js/Routes
                    icon: "bx bx-file", // Icon for the new menu
                    module: []
                };

                if (arBookIndex !== -1) {
                    // Insert AT position index + 1 (Right below AR Book)
                    reportsModule.screen.splice(arBookIndex + 1, 0, newArBookDo);
                } else {
                    // Fallback: If "AR Book" isn't found, just add it to the end of Reports
                    reportsModule.screen.push(newArBookDo);
                }
            }
        }

        // ---------------------------------------------------------

        // 4. Your existing Test Menu Logic (Mktg Verify)
        const testMenu = {
            moduleId: 9999,
            moduleName: "Mktg Verify",
            icon: "bx bx-test-tube",
            screen: [
                {
                    screenId: 99901,
                    screenName: "Verify Customer",
                    url: "/verify-customer",
                    icon: "bx bx-file",
                    module: []
                }
            ]
        };

        // Add your test menu if it's not already there
        if (!menuData.menus.find(m => m.moduleId === 9999)) {
            menuData.menus.push(testMenu);
        }

        // 5. Update State
        this.setState({ dynamicMenu: menuData || [] }, () => {
            this.initMenu();
        });

        console.log("Loaded Menu with Injections:", menuData);
    };

    // eslint-disable-next-line no-unused-vars
    componentDidUpdate(prevProps, prevState, ss) {
        if (this.props.type !== prevProps.type) {
            this.initMenu();
        }
    }

    initMenu() {
        new MetisMenu("#side-menu");

        let matchingMenuItem = null;
        const ul = document.getElementById("side-menu");
        if (ul) {
            const items = ul.getElementsByTagName("a");
            for (let i = 0; i < items.length; ++i) {
                if (this.props.location.pathname === items[i].pathname) {
                    matchingMenuItem = items[i];
                    break;
                }
            }
            if (matchingMenuItem) {
                this.activateParentDropdown(matchingMenuItem);
            }
        }
    }

    scrollElement = item => {
        setTimeout(() => {
            if (this.refDiv.current !== null) {
                if (item) {
                    const currentPosition = item.offsetTop;
                    if (currentPosition > window.innerHeight) {
                        if (this.refDiv.current)
                            this.refDiv.current.getScrollElement().scrollTop =
                                currentPosition - 300;
                    }
                }
            }
        }, 300);
    };

    activateParentDropdown = item => {
        item.classList.add("active");
        const parent = item.parentElement;

        const parent2El = parent.childNodes[1];
        if (parent2El && parent2El.id !== "side-menu") {
            parent2El.classList.add("mm-show");
        }

        if (parent) {
            parent.classList.add("mm-active");
            const parent2 = parent.parentElement;

            if (parent2) {
                parent2.classList.add("mm-show"); // ul tag

                const parent3 = parent2.parentElement; // li tag

                if (parent3) {
                    parent3.classList.add("mm-active"); // li
                    parent3.childNodes[0].classList.add("mm-active"); //a
                    const parent4 = parent3.parentElement; // ul
                    if (parent4) {
                        parent4.classList.add("mm-show"); // ul
                        const parent5 = parent4.parentElement;
                        if (parent5) {
                            parent5.classList.add("mm-show"); // li
                            parent5.childNodes[0].classList.add("mm-active"); // a tag
                        }
                    }
                }
            }
            this.scrollElement(item);
            return false;
        }
        this.scrollElement(item);
        return false;
    };


    renderDynamicMenu = (menus) => {
        return menus.map((menu) => (
            <li key={menu.moduleId}>
                <Link to="/#" className="has-arrow">
                    <i className={menu.icon}></i>
                    <span>{menu.moduleName}</span>
                </Link>
                <ul className="sub-menu" aria-expanded="false">
                    {menu.screen.map((screen) => (
                        <React.Fragment key={screen.screenId}>
                            <li>
                                <Link to={screen.url}>
                                    <i className={screen.icon}></i>
                                    {screen.screenName}
                                </Link>
                            </li>
                            {/* Render nested modules if any */}
                            {screen.module && screen.module.length > 0 && screen.module.map((subModule) => (
                                <li key={subModule.moduleId}>
                                    <Link to="/#" className="has-arrow">
                                        <i className={subModule.icon}></i>
                                        {subModule.moduleName}
                                    </Link>
                                    <ul className="sub-menu" aria-expanded="false">
                                        {subModule.screen.map((nestedScreen) => (
                                            <li key={nestedScreen.screenId}>
                                                <Link to={nestedScreen.url}>
                                                    <i className={nestedScreen.icon}></i>
                                                    {nestedScreen.screenName}
                                                </Link>
                                            </li>
                                        ))}
                                    </ul>
                                </li>
                            ))}
                        </React.Fragment>
                    ))}
                </ul>
            </li>
        ));
    };

    render() {
        return (
            <React.Fragment>
                <SimpleBar className="h-100" ref={this.refDiv}>
                    <div id="sidebar-menu">
                        {/* <ul className="metismenu list-unstyled" id="side-menu">
                          
                            <li className="menu-title"></li>
                            <li>
                                <Link to="/#" className="has-arrow">
                                    <i className="bx bx-shopping-bag" />
                                    <span>Admin</span>
                                </Link>
                                <ul className="sub-menu" aria-expanded="false">
                                    <li>
                                        <Link to="/company">
                                            <i className="bx bx-notepad" />
                                            Company
                                        </Link>
                                    </li>
                                </ul>
                            </li>

                            <li>
                                <Link to="/#" className="has-arrow">
                                    <i className="bx bx-receipt" />
                                    <span>Masters</span>
                                </Link>
                                <ul className="sub-menu" aria-expanded="false">
                                    <li>
                                        <Link to="/admin-roles">
                                            <i className="bx bx-group" />
                                            Access Rights
                                        </Link>
                                    </li>
                                    <li>
                                        <Link to="/country">
                                            <i className="bx bx-flag" />
                                            Country
                                        </Link>
                                    </li>
                                    <li>
                                        <Link to="/currency">
                                            <i className="bx bx-money" />
                                            Currency
                                        </Link>
                                    </li>
                                    <li>
                                        <Link to="/manage-customer">
                                            <i className="bx bx-group" />
                                            Customers
                                        </Link>
                                    </li>
                                    <li>
                                        <Link to="/manage-cylinder">
                                            <i className="bx bx-data" />
                                            Cylinder
                                        </Link>
                                    </li>
                                    <li>
                                        <Link to="/department">
                                            <i className="bx bxs-buildings" />
                                            Departments
                                        </Link>
                                    </li>
                                    <li>
                                        <Link to="/manage-gas">
                                            <i className="bx bxs-gas-pump" />
                                            Gas
                                        </Link>
                                    </li>
                                    <li>
                                        <Link to="/manage-payment-methods">
                                            <i className="bx bx-money" />
                                            Payment Methods
                                        </Link>
                                    </li>
                                    <li>
                                        <Link to="/manage-payment-terms">
                                            <i className="bx bx-cart-alt" />
                                            Payment Terms
                                        </Link>
                                    </li>
                                    <li>
                                        <Link to="/manage-pallet">
                                            <i className="bx bx-list-ul" />
                                            Pallet
                                        </Link>
                                    </li>
                                    <li>
                                        <Link to="/manage-suppliers">
                                            <i className="bx bx-id-card" />
                                            Suppliers
                                        </Link>
                                    </li>
                                    <li>
                                        <Link to="/manage-units">
                                            <i className="bx bxs-file-plus" />
                                            UOM
                                        </Link>
                                    </li>
                                    <li>
                                        <Link to="/manage-users">
                                            <i className="bx bxs-layer-plus" />
                                            Users
                                        </Link>
                                    </li>
                                </ul>
                            </li>

                     
                            <li>
                                <Link to="/#" className="has-arrow">
                                    <i className="bx bxs-detail" />
                                    <span>Sales</span>
                                </Link>
                                <ul className="sub-menu" aria-expanded="false">
                                    <li>
                                        <Link to="/manage-quotation">
                                            <i className="bx bx-notepad" />
                                            Quotations
                                        </Link>
                                    </li>
                                    <li>
                                        <Link to="/manage-order">
                                            <i className="bx bx-dice-1" />
                                            Orders
                                        </Link>
                                    </li>

                                    <li>
    <Link to="/#" className="has-arrow">
        <i className="bx bx-transfer" />
        <span>Distribution</span>
    </Link>
    <ul className="sub-menu" aria-expanded="false">
        <li>
            <Link to="/TransportPlanner">
                <i className="bx bx-clipboard" />
                Master SO
            </Link>
        </li>
        <li>
            <Link to="/PackingList">
                <i className="bx bx-package" />
                Packing List
            </Link>
        </li>
    </ul>
</li>

                               
                                    <li>
                                        <Link to="/manage-packing">
                                            <i className="bx bx-package" />
                                            Delivery Order
                                        </Link>
                                    </li>
                                    <li>
                                        <Link to="/sales-invoices">
                                            <i className="bx bx-spreadsheet" />
                                            Sales Invoice
                                        </Link>
                                    </li>
                                    <li>
                                        <Link to="/sales-return">
                                            <i className="bx bx-redo" />
                                            Return Order
                                        </Link>
                                    </li>
                                    <li>
                                        <Link to="/production-order">
                                            <i className="bx bx-food-menu" />
                                            Production Order
                                        </Link>
                                    </li>
                                </ul>
                            </li>

                          
                            <li>
                                <Link to="/#" className="has-arrow">
                                    <i className="bx bx-shopping-bag" />
                                    <span>Procurement</span>
                                </Link>
                                <ul className="sub-menu" aria-expanded="false">
                                    <li>
                                        <Link to="/procurementspurchase-memo">

                                            <i className="bx bx-notepad" />
                                            Purchase Memo
                                        </Link>
                                    </li>
                                    <li>
                                        <Link to="/procurementspurchase-requisition">
                                            <i className="bx bx-notepad" />
                                            Purchase Requisition
                                        </Link>
                                    </li>
                                    <li>
                                        <Link to="/procurementspurchase-order">
                                            <i className="bx bx-file" />
                                            Purchase Order
                                        </Link>
                                    </li>
                                    <li>
                                        <Link to="/procurementsgrn">
                                            <i className="bx bx-clipboard" />
                                            GRN
                                        </Link>
                                    </li>
                                </ul>
                            </li>
                            <li>
                                <Link to="/#" className="has-arrow">
                                    <i className="bx bx-shopping-bag" />
                                    <span>Finance</span>
                                </Link>
                                <ul className="sub-menu" aria-expanded="false">
                                    <li>
                                        <Link to="/Manageclaim&Payment">
                                            <i className="bx bx-notepad" />
                                            Claim & Payment
                                        </Link>
                                    </li>
                                   
                                    <li>
                                        <Link to="/Paymentplanapproval">
                                            <i className="bx bx-clipboard" />
                                           Payment Plan
                                        </Link>
                                    </li>
                                    <li>
                                        <Link to="/PPP">
                                            <i className="bx bx-clipboard" />
                                           PPP
                                        </Link>
                                    </li>
                                     
                                     <li>
                                        <Link to="/Manageapproval">
                                            <i className="bx bx-clipboard" />
                                            Approval
                                        </Link>
                                    </li>
                                    
                                    
                                    
                                </ul>
                            </li>
                        </ul>
  */}
                        <ul className="metismenu list-unstyled" id="side-menu">
                            <li className="menu-title"></li>
                            {this.state.dynamicMenu && this.state.dynamicMenu.menus && this.renderDynamicMenu(this.state.dynamicMenu.menus)}
                        </ul>
                    </div>
                </SimpleBar>
            </React.Fragment>
        );
    }
}

SidebarContent.propTypes = {
    location: PropTypes.object,
    t: PropTypes.any,
    type: PropTypes.string,
};

export default withRouter(withTranslation()(SidebarContent));